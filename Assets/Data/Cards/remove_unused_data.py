import os
import json

def remove_legalities_from_json(file_path):
    with open(file_path, 'r') as file:
        data = json.load(file)
    
    for card in data:
        if 'level' in card:
            del card['level']
        if 'artist' in card:
            del card['artist']
        if 'flavorText' in card:
            del card['flavorText']
        if 'legalities' in card:
            del card['legalities']
        if 'nationalPokedexNumbers' in card:
            del card['nationalPokedexNumbers']
    
    with open(file_path, 'w') as file:
        json.dump(data, file, indent=2)

def process_cards_folder(folder_path):
    for root, _, files in os.walk(folder_path):
        for file in files:
            if file.endswith('.json'):
                file_path = os.path.join(root, file)
                remove_legalities_from_json(file_path)

cards_folder_path = 'Assets/Data/Cards'
process_cards_folder(cards_folder_path)