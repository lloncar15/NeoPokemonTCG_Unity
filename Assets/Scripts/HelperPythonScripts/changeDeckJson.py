import os
import json

def process_deck_json(file_path, set_ids, deck_id):
    with open(file_path, 'r', encoding='utf-8') as file:
        data = json.load(file)

    for deck in data:
        # Update the top-level deck ID
        if "id" in deck:
            try:
                # Extract the set string and deck number from the current ID
                parts = deck["id"].split("-")
                set_string = parts[1]  # e.g., "base1"
                deck_number = int(parts[-1])  # e.g., "1"

                # Look up the set ID from the dictionary
                if set_string in set_ids:
                    set_id = set_ids[set_string]
                else:
                    print(f"Error: Set ID for '{set_string}' not found in dictionary.")
                    continue

                # Calculate the new ID
                new_deck_id = f"{set_id + deck_id + deck_number}"
                deck["id"] = new_deck_id
            except (ValueError, IndexError):
                print(f"Error: Invalid deck ID format in {deck['id']}")
                continue

        # Process the cards list
        if "cards" in deck:
            for card in deck["cards"]:
                # Update the card ID
                if "id" in card:
                    try:
                        # Extract the set string and card number from the current ID
                        parts = card["id"].split("-")
                        set_string = parts[0]  # e.g., "base1"
                        card_number = int(parts[-1])  # e.g., "47"

                        # Look up the set ID from the dictionary
                        if set_string in set_ids:
                            set_id = set_ids[set_string]
                        else:
                            print(f"Error: Set ID for '{set_string}' not found in dictionary.")
                            continue

                        # Calculate the new card ID
                        new_card_id = f"{set_id + card_number}"
                        card["id"] = new_card_id
                    except (ValueError, IndexError):
                        print(f"Error: Invalid card ID format in {card['id']}")
                        continue

                # Remove the `rarity` and `name` keys
                card.pop("rarity", None)
                card.pop("name", None)

    # Save the updated JSON back to the file
    with open(file_path, 'w', encoding='utf-8') as file:
        json.dump(data, file, indent=2)
    print(f"Processed file: {file_path}")

# Dictionary of set IDs
set_ids = {
    "base1": 1000,
    "base2": 2000,
    "base3": 3000,
    "base4": 4000,
    "base5": 5000,
    "base6": 6000,
    "gym1": 7000,
    "gym2": 8000,
    "neo1": 9000,
    "neo2": 10000,
    "neo3": 11000,
    "neo4": 12000,
    "ecard1": 13000,
    "ecard2": 14000,
    "ecard3": 15000,
    "basep": 16000,
    "bp": 17000,
    "si1": 18000
}

# Specify the JSON file path and deck ID
json_file_path = r"d:\Apps\Unity\Projects\NeoPokemonTCG_Unity\Assets\Data\Decks\base1.json"
deck_id = 900  # Example deck ID

# Run the function
process_deck_json(json_file_path, set_ids, deck_id)