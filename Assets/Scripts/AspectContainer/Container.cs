using System.Collections.Generic;
using System.Linq;

namespace GimGim.AspectContainer {
    public interface IContainer {
        T AddAspect<T>(string key = null) where T : IAspect, new();
        T AddAspect<T>(T aspect, string key = null) where T : IAspect;
        T GetAspect<T>(string key = null) where T : IAspect;
        ICollection<IAspect> Aspects();
    }

    public class Container : IContainer {
        private readonly Dictionary<string, IAspect> _aspects = new Dictionary<string, IAspect>();
        
        public T AddAspect<T>(string key = null) where T : IAspect, new() {
            return AddAspect<T>(new T (), key);
        }

        public T AddAspect<T>(T aspect, string key = null) where T : IAspect {
            key = key ?? typeof(T).Name;
            _aspects.Add(key, aspect);
            aspect.Container = this;
            return aspect;
        }
        
        public T GetAspect<T>(string key = null) where T : IAspect {
            key = key ?? typeof(T).Name;
            T aspect = _aspects.TryGetValue(key, out var aspectLookup) ? (T)aspectLookup :default(T);
            return aspect;
        }
        
        public ICollection<IAspect> Aspects() {
            return _aspects.Values;
        }
    }
}