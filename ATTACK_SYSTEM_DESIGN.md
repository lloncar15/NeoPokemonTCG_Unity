# Attack System Design - Hybrid Composite + Chain Pattern

## Overview

This document outlines the implementation of a modular, scalable attack system for the Pokemon TCG Unity project. The system uses a hybrid approach combining the Chain of Responsibility pattern for linear effect execution with the Composite pattern for conditional branching.

## Architecture

### Core Components

1. **AttackEffect** - Base class for all attack effects (Chain Pattern)
2. **AttackAction** - Main orchestrator integrating with ActionSystem (Composite Pattern)
3. **AttackContext** - Shared state object passed through effect chain
4. **AttackProfile** - JSON-based attack definitions
5. **AttackBuilder** - Factory for runtime attack creation

## Implementation

### 1. AttackEffect Base Class (Chain Pattern)

```csharp
using System.Collections;
using GimGim.AspectContainer;

namespace GimGim.ActionSystem.Attacks {
    /// <summary>
    /// Base class for all attack effects. Effects form a chain where each effect
    /// can modify the AttackContext and decide whether to continue execution.
    /// </summary>
    public abstract class AttackEffect {
        public string EffectId { get; protected set; }
        public AttackEffect NextEffect { get; set; }
        
        protected AttackEffect(string effectId) {
            EffectId = effectId;
        }
        
        /// <summary>
        /// Executes this effect and optionally continues the chain.
        /// </summary>
        /// <param name="context">Shared context containing attack state</param>
        /// <param name="game">Game container for accessing systems</param>
        /// <returns>IEnumerator for coroutine execution</returns>
        public IEnumerator Execute(AttackContext context, IContainer game) {
            // Execute this effect
            yield return ExecuteEffect(context, game);
            
            // Continue chain if not interrupted and next effect exists
            if (!context.IsInterrupted && NextEffect != null) {
                yield return NextEffect.Execute(context, game);
            }
        }
        
        /// <summary>
        /// Override this to implement the specific effect logic.
        /// </summary>
        protected abstract IEnumerator ExecuteEffect(AttackContext context, IContainer game);
        
        /// <summary>
        /// Adds an effect to the end of the chain.
        /// </summary>
        public AttackEffect AddNext(AttackEffect effect) {
            if (NextEffect == null) {
                NextEffect = effect;
            } else {
                NextEffect.AddNext(effect);
            }
            return this;
        }
    }
}
```

### 2. AttackContext (Shared State)

```csharp
using System.Collections.Generic;
using GimGim.Model;

namespace GimGim.ActionSystem.Attacks {
    /// <summary>
    /// Shared context passed through the effect chain, containing all attack state.
    /// </summary>
    public class AttackContext {
        public Card AttackingCard { get; }
        public Card DefendingCard { get; }
        public int BaseDamage { get; set; }
        public int FinalDamage { get; set; }
        public bool IsInterrupted { get; set; }
        
        // Results storage for conditional effects
        public Dictionary<string, object> Results { get; } = new();
        
        // Status conditions to apply
        public List<StatusCondition> StatusesToApply { get; } = new();
        
        public AttackContext(Card attackingCard, Card defendingCard, int baseDamage = 0) {
            AttackingCard = attackingCard;
            DefendingCard = defendingCard;
            BaseDamage = baseDamage;
            FinalDamage = baseDamage;
        }
        
        public T GetResult<T>(string effectId, T defaultValue = default) {
            return Results.TryGetValue(effectId, out var result) ? (T)result : defaultValue;
        }
        
        public void SetResult(string effectId, object result) {
            Results[effectId] = result;
        }
    }
    
    public enum StatusCondition {
        Paralyzed,
        Confused,
        Poisoned,
        Asleep,
        Burned
    }
}
```

### 3. AttackAction (Composite Pattern)

```csharp
using System.Collections;
using System.Collections.Generic;
using GimGim.AspectContainer;
using GimGim.EventSystem;
using GimGim.Model;

namespace GimGim.ActionSystem.Attacks {
    /// <summary>
    /// Main attack action that orchestrates the execution of effect chains.
    /// Integrates with the existing ActionSystem phases.
    /// </summary>
    public class AttackAction : GameAction {
        private readonly AttackEffect _effectChain;
        private readonly AttackContext _context;
        private readonly List<AttackAction> _subActions = new();
        
        public AttackContext Context => _context;
        public string AttackName { get; }
        
        public AttackAction(object sender, string attackName, Card attackingCard, Card defendingCard, 
                           AttackEffect effectChain, int baseDamage = 0) 
            : base(sender) {
            AttackName = attackName;
            _effectChain = effectChain;
            _context = new AttackContext(attackingCard, defendingCard, baseDamage);
        }
        
        protected override void OnPrepare(IContainer game) {
            base.OnPrepare(game);
            // Post attack preparation event for reactions
            NotificationEventSystem.PostEventAndExecute(
                new AttackPreparedEvent(Sender, this, _context));
        }
        
        protected override void OnPerform(IContainer game) {
            // Execute the effect chain
            if (_effectChain != null) {
                // Start coroutine execution (would be handled by ActionSystem)
                game.StartCoroutine(ExecuteEffectChain(game));
            }
            
            base.OnPerform(game);
        }
        
        private IEnumerator ExecuteEffectChain(IContainer game) {
            yield return _effectChain.Execute(_context, game);
            
            // Execute any sub-actions that were generated during effect execution
            foreach (var subAction in _subActions) {
                // Sub-actions would be queued in the ActionSystem
                NotificationEventSystem.PostEventAndExecute(
                    new GameActionRequestedEvent(Sender, subAction));
                yield return new WaitUntil(() => !subAction.IsActive);
            }
            
            // Post final attack completion event
            NotificationEventSystem.PostEventAndExecute(
                new AttackCompletedEvent(Sender, this, _context));
        }
        
        /// <summary>
        /// Adds a sub-action to be executed after the main effect chain.
        /// Used by effects that need to trigger additional actions.
        /// </summary>
        public void AddSubAction(AttackAction subAction) {
            _subActions.Add(subAction);
        }
    }
    
    // Event classes for attack lifecycle
    public class AttackPreparedEvent : EventData {
        public AttackAction Attack { get; }
        public AttackContext Context { get; }
        
        public AttackPreparedEvent(object sender, AttackAction attack, AttackContext context) 
            : base(sender) {
            Attack = attack;
            Context = context;
        }
    }
    
    public class AttackCompletedEvent : EventData {
        public AttackAction Attack { get; }
        public AttackContext Context { get; }
        
        public AttackCompletedEvent(object sender, AttackAction attack, AttackContext context) 
            : base(sender) {
            Attack = attack;
            Context = context;
        }
    }
}
```

### 4. Concrete Effect Examples

#### CoinFlipEffect
```csharp
using System.Collections;
using GimGim.AspectContainer;
using UnityEngine;

namespace GimGim.ActionSystem.Attacks.Effects {
    public class CoinFlipEffect : AttackEffect {
        public CoinFlipEffect(string effectId) : base(effectId) { }
        
        protected override IEnumerator ExecuteEffect(AttackContext context, IContainer game) {
            // Simulate coin flip
            bool isHeads = Random.Range(0, 2) == 0;
            
            // Store result for conditional effects
            context.SetResult(EffectId, isHeads);
            
            // Post event for UI updates, reactions, etc.
            NotificationEventSystem.PostEventAndExecute(
                new CoinFlipEvent(context.AttackingCard, EffectId, isHeads));
            
            // Could add delay for animation
            yield return new WaitForSeconds(0.5f);
        }
    }
}
```

#### ConditionalEffect (Composite within Chain)
```csharp
using System.Collections;
using System.Collections.Generic;
using GimGim.AspectContainer;

namespace GimGim.ActionSystem.Attacks.Effects {
    /// <summary>
    /// Conditional effect that executes child effects based on conditions.
    /// This is where the composite pattern integrates with the chain.
    /// </summary>
    public class ConditionalEffect : AttackEffect {
        private readonly string _conditionEffectId;
        private readonly object _expectedValue;
        private readonly List<AttackEffect> _conditionalEffects = new();
        
        public ConditionalEffect(string effectId, string conditionEffectId, object expectedValue) 
            : base(effectId) {
            _conditionEffectId = conditionEffectId;
            _expectedValue = expectedValue;
        }
        
        public ConditionalEffect AddConditionalEffect(AttackEffect effect) {
            _conditionalEffects.Add(effect);
            return this;
        }
        
        protected override IEnumerator ExecuteEffect(AttackContext context, IContainer game) {
            // Check condition
            var actualValue = context.GetResult<object>(_conditionEffectId);
            bool conditionMet = actualValue?.Equals(_expectedValue) ?? false;
            
            context.SetResult(EffectId, conditionMet);
            
            if (conditionMet) {
                // Execute all conditional effects in sequence
                foreach (var effect in _conditionalEffects) {
                    yield return effect.Execute(context, game);
                    
                    // Stop if chain was interrupted
                    if (context.IsInterrupted) break;
                }
            }
        }
    }
}
```

#### StatusConditionEffect
```csharp
using System.Collections;
using GimGim.AspectContainer;

namespace GimGim.ActionSystem.Attacks.Effects {
    public class StatusConditionEffect : AttackEffect {
        private readonly StatusCondition _statusCondition;
        
        public StatusConditionEffect(string effectId, StatusCondition statusCondition) 
            : base(effectId) {
            _statusCondition = statusCondition;
        }
        
        protected override IEnumerator ExecuteEffect(AttackContext context, IContainer game) {
            // Add status condition to be applied
            context.StatusesToApply.Add(_statusCondition);
            context.SetResult(EffectId, _statusCondition);
            
            // Post event for systems to react
            NotificationEventSystem.PostEventAndExecute(
                new StatusConditionAppliedEvent(context.DefendingCard, _statusCondition));
            
            yield return null;
        }
    }
}
```

#### DamageEffect
```csharp
using System.Collections;
using GimGim.AspectContainer;

namespace GimGim.ActionSystem.Attacks.Effects {
    public class DamageEffect : AttackEffect {
        private readonly int _baseDamage;
        private readonly bool _useContextDamage;
        
        public DamageEffect(string effectId, int baseDamage = 0, bool useContextDamage = true) 
            : base(effectId) {
            _baseDamage = baseDamage;
            _useContextDamage = useContextDamage;
        }
        
        protected override IEnumerator ExecuteEffect(AttackContext context, IContainer game) {
            int damage = _useContextDamage ? context.FinalDamage : _baseDamage;
            
            // Apply damage calculations, weakness/resistance, etc.
            int finalDamage = CalculateFinalDamage(damage, context, game);
            
            context.FinalDamage = finalDamage;
            context.SetResult(EffectId, finalDamage);
            
            // Post damage event for damage system to handle
            NotificationEventSystem.PostEventAndExecute(
                new DamageDealtEvent(context.AttackingCard, context.DefendingCard, finalDamage));
            
            yield return null;
        }
        
        private int CalculateFinalDamage(int baseDamage, AttackContext context, IContainer game) {
            // This would integrate with existing damage calculation systems
            // Apply weakness, resistance, damage modifiers, etc.
            return baseDamage; // Simplified for example
        }
    }
}
```

### 5. AttackProfile and Builder (Factory Pattern)

#### AttackProfile (Enhanced JSON Structure)
```csharp
using System.Collections.Generic;
using GimGim.Serialization;

namespace GimGim.Data.Attacks {
    public class AttackProfile : Profile {
        private string _name;
        private int _baseDamage;
        private List<string> _energyCost = new();
        private string _description;
        private List<AttackEffectData> _effects = new();
        
        public string Name => _name;
        public int BaseDamage => _baseDamage;
        public List<string> EnergyCost => _energyCost;
        public string Description => _description;
        public List<AttackEffectData> Effects => _effects;
        
        public override bool Decode(IDecoder decoder) {
            bool success = base.Decode(decoder);
            
            success &= decoder.Get("name", ref _name);
            decoder.Get("baseDamage", ref _baseDamage, 0);
            decoder.Get("energyCost", ref _energyCost, new List<string>());
            decoder.Get("description", ref _description, "");
            decoder.Get("effects", ref _effects, new List<AttackEffectData>());
            
            return success;
        }
    }
    
    [System.Serializable]
    public class AttackEffectData {
        public string type;
        public string id;
        public Dictionary<string, object> parameters = new();
        public List<AttackEffectData> conditionalEffects = new();
    }
}
```

#### AttackBuilder (Factory + Builder Pattern)
```csharp
using System.Collections.Generic;
using GimGim.ActionSystem.Attacks;
using GimGim.ActionSystem.Attacks.Effects;
using GimGim.Data.Attacks;
using GimGim.Model;

namespace GimGim.Data.Factories {
    /// <summary>
    /// Factory class for building AttackActions from AttackProfiles at runtime.
    /// Implements object pooling for memory efficiency.
    /// </summary>
    public class AttackBuilder {
        private readonly Dictionary<string, System.Func<AttackEffectData, AttackEffect>> _effectFactories;
        private readonly EffectPool _effectPool;
        
        public AttackBuilder() {
            _effectPool = new EffectPool();
            _effectFactories = new Dictionary<string, System.Func<AttackEffectData, AttackEffect>> {
                ["coinFlip"] = CreateCoinFlipEffect,
                ["conditional"] = CreateConditionalEffect,
                ["statusCondition"] = CreateStatusConditionEffect,
                ["damage"] = CreateDamageEffect
            };
        }
        
        public AttackAction BuildAttack(AttackProfile profile, Card attackingCard, Card defendingCard) {
            // Build effect chain from profile
            AttackEffect effectChain = BuildEffectChain(profile.Effects);
            
            // Create the main attack action
            return new AttackAction(
                attackingCard, 
                profile.Name, 
                attackingCard, 
                defendingCard, 
                effectChain, 
                profile.BaseDamage
            );
        }
        
        private AttackEffect BuildEffectChain(List<AttackEffectData> effectsData) {
            if (effectsData == null || effectsData.Count == 0) return null;
            
            AttackEffect firstEffect = null;
            AttackEffect currentEffect = null;
            
            foreach (var effectData in effectsData) {
                var effect = CreateEffect(effectData);
                if (effect == null) continue;
                
                if (firstEffect == null) {
                    firstEffect = currentEffect = effect;
                } else {
                    currentEffect.NextEffect = effect;
                    currentEffect = effect;
                }
            }
            
            return firstEffect;
        }
        
        private AttackEffect CreateEffect(AttackEffectData data) {
            if (!_effectFactories.TryGetValue(data.type, out var factory)) {
                return null; // Unknown effect type
            }
            
            return factory(data);
        }
        
        private AttackEffect CreateCoinFlipEffect(AttackEffectData data) {
            return _effectPool.GetCoinFlipEffect(data.id);
        }
        
        private AttackEffect CreateConditionalEffect(AttackEffectData data) {
            var conditionEffectId = data.parameters["conditionEffectId"] as string;
            var expectedValue = data.parameters["expectedValue"];
            
            var conditional = new ConditionalEffect(data.id, conditionEffectId, expectedValue);
            
            // Add conditional effects
            foreach (var childData in data.conditionalEffects) {
                var childEffect = CreateEffect(childData);
                if (childEffect != null) {
                    conditional.AddConditionalEffect(childEffect);
                }
            }
            
            return conditional;
        }
        
        private AttackEffect CreateStatusConditionEffect(AttackEffectData data) {
            var statusName = data.parameters["status"] as string;
            if (System.Enum.TryParse<StatusCondition>(statusName, out var status)) {
                return new StatusConditionEffect(data.id, status);
            }
            return null;
        }
        
        private AttackEffect CreateDamageEffect(AttackEffectData data) {
            var baseDamage = data.parameters.ContainsKey("baseDamage") 
                ? (int)data.parameters["baseDamage"] : 0;
            var useContext = data.parameters.ContainsKey("useContextDamage") 
                ? (bool)data.parameters["useContextDamage"] : true;
                
            return new DamageEffect(data.id, baseDamage, useContext);
        }
    }
    
    /// <summary>
    /// Object pool for commonly used effects to reduce memory allocation.
    /// </summary>
    public class EffectPool {
        private readonly Queue<CoinFlipEffect> _coinFlipPool = new();
        
        public CoinFlipEffect GetCoinFlipEffect(string id) {
            if (_coinFlipPool.Count > 0) {
                var effect = _coinFlipPool.Dequeue();
                // Reset effect with new ID
                return effect;
            }
            return new CoinFlipEffect(id);
        }
        
        public void ReturnCoinFlipEffect(CoinFlipEffect effect) {
            _coinFlipPool.Enqueue(effect);
        }
    }
}
```

## Example Usage

### JSON Profile for Complex Attack
```json
{
  "id": "confuse-ray-attack",
  "name": "Confuse Ray",
  "baseDamage": 30,
  "energyCost": ["Psychic", "Psychic", "Psychic"],
  "description": "Flip a coin. If it's heads, flip another coin. If the second coin is heads the defending pokemon is paralyzed",
  "effects": [
    {
      "type": "coinFlip",
      "id": "flip1",
      "parameters": {}
    },
    {
      "type": "conditional",
      "id": "firstFlipCheck",
      "parameters": {
        "conditionEffectId": "flip1",
        "expectedValue": true
      },
      "conditionalEffects": [
        {
          "type": "coinFlip",
          "id": "flip2",
          "parameters": {}
        },
        {
          "type": "conditional",
          "id": "secondFlipCheck",
          "parameters": {
            "conditionEffectId": "flip2",
            "expectedValue": true
          },
          "conditionalEffects": [
            {
              "type": "statusCondition",
              "id": "paralyzedStatus",
              "parameters": {
                "status": "Paralyzed"
              }
            }
          ]
        }
      ]
    },
    {
      "type": "damage",
      "id": "baseDamage",
      "parameters": {
        "useContextDamage": true
      }
    }
  ]
}
```

### Runtime Usage
```csharp
// In your game system when an attack is triggered
public class AttackSystem : GameplaySystem {
    private AttackBuilder _attackBuilder;
    
    public void Awake() {
        base.Awake();
        _attackBuilder = new AttackBuilder();
    }
    
    public void ExecuteAttack(Card attackingCard, Card defendingCard, string attackName) {
        // Load attack profile (from existing profile system)
        var attackProfile = ProfilesController.GetAttackProfile(attackingCard.Id, attackName);
        
        // Build attack action at runtime
        var attackAction = _attackBuilder.BuildAttack(attackProfile, attackingCard, defendingCard);
        
        // Execute through existing ActionSystem
        NotificationEventSystem.PostEventAndExecute(
            new GameActionRequestedEvent(this, attackAction));
    }
}
```

### Execution Flow Example
```
1. AttackAction.OnPrepare() → Posts AttackPreparedEvent
2. AttackAction.OnPerform() → Starts effect chain execution
3. CoinFlipEffect("flip1") → Flips coin, stores result
4. ConditionalEffect("firstFlipCheck") → Checks flip1 == true
   - If true: Executes conditional effects
     5. CoinFlipEffect("flip2") → Second coin flip
     6. ConditionalEffect("secondFlipCheck") → Checks flip2 == true
        - If true: StatusConditionEffect("paralyzedStatus")
5. DamageEffect("baseDamage") → Applies 30 base damage
6. AttackAction completes → Posts AttackCompletedEvent
```

## Key Benefits

### 🔗 Chain Pattern Benefits:
- **Linear execution** with clear data flow through AttackContext
- **Easy debugging** - each effect can be traced individually
- **Flexible ordering** - effects execute in defined sequence
- **Interruption support** - chain can be stopped at any point

### 🏗️ Composite Pattern Benefits:
- **Nested conditions** - ConditionalEffect can contain sub-effects
- **Complex branching** - Multiple conditional paths within single attack
- **Reusable components** - Effects can be composed in different ways
- **Hierarchical structure** - Natural tree-like effect organization

### 🔄 Hybrid Integration:
- **Best of both worlds** - Chain for sequence, Composite for conditions
- **ActionSystem integration** - Leverages existing coroutine execution
- **Event-driven** - Integrates with NotificationEventSystem
- **Memory efficient** - Runtime creation with object pooling

### 📈 Scalability Features:
- **Extensible** - New effects added without touching core system
- **Data-driven** - Complex attacks defined in JSON
- **Performance optimized** - Object pooling and lazy loading
- **Memory managed** - Match-scoped lifecycle prevents leaks

## Implementation Notes

1. **Memory Management**: Objects are created at runtime when cards enter play and destroyed when match ends
2. **Event Integration**: All effects post events for UI updates and system reactions
3. **Extensibility**: New effect types can be added by implementing AttackEffect and registering in AttackBuilder
4. **Performance**: Object pooling reduces garbage collection for common effects
5. **Debugging**: Each effect has unique ID for tracing execution flow

This hybrid approach provides the flexibility to handle both simple linear effects and complex conditional branching while maintaining performance and architectural consistency with the existing codebase.