import os
import json

def update_ids_in_json(file_path, base_id):
    with open(file_path, 'r', encoding='utf-8') as file:
        data = json.load(file)

    index = 0  # Start the index from 0

    for item in data:
        index += 1  # Increment the index for each card
        if "number" in item:
            try:
                # Use the item's "number" field to calculate the new ID if it's an integer
                new_id = f"{base_id + int(item['number']):04d}"  # Format as a 4-digit number
            except ValueError:
                # If "number" is not an integer, use the index instead
                new_id = f"{base_id + index:04d}"
        else:
            # If "number" is missing, use the index instead
            new_id = f"{base_id + index:04d}"

        item["id"] = new_id  # Assign the new ID

    # Save the updated JSON back to the file
    with open(file_path, 'w', encoding='utf-8') as file:
        json.dump(data, file, indent=2)
    print(f"Updated IDs in file: {file_path}")

# Specify the JSON file path and base ID
json_file_path = r"d:\Apps\Unity\Projects\NeoPokemonTCG_Unity\Assets\Data\Cards\base1.json"
base_id = 1000

# Run the function
update_ids_in_json(json_file_path, base_id)