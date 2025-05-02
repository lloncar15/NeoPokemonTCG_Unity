import os
import json

def combine_and_sort_deck_profiles(folder_path, output_file):
    combined_data = []

    # Iterate through all JSON files in the folder
    for filename in os.listdir(folder_path):
        if filename.endswith(".json"):
            file_path = os.path.join(folder_path, filename)

            # Extract the setCode from the filename (e.g., base1.json → base1)
            set_code = os.path.splitext(filename)[0]

            # Load the JSON file
            with open(file_path, 'r', encoding='utf-8') as file:
                try:
                    data = json.load(file)
                    # Add the "setCode" key to each item
                    for item in data:
                        item["setCode"] = set_code
                    combined_data.extend(data)  # Add the data to the combined list
                except json.JSONDecodeError:
                    print(f"Error decoding JSON in file: {filename}")
                    continue

    # Sort the combined data by the "id" field numerically
    combined_data.sort(key=lambda x: int(x["id"]))

    # Write the combined and sorted data to the output file
    with open(output_file, 'w', encoding='utf-8') as file:
        json.dump(combined_data, file, indent=2)
    print(f"Combined and sorted deck profiles saved to: {output_file}")

# Specify the folder path and output file
deck_profiles_folder = r"d:\Apps\Unity\Projects\NeoPokemonTCG_Unity\Assets\Resources\Profiles\CardProfiles"
output_file = r"d:\Apps\Unity\Projects\NeoPokemonTCG_Unity\Assets\Resources\Profiles\DeckProfiles\cardProfiles.json"

# Run the function
combine_and_sort_deck_profiles(deck_profiles_folder, output_file)