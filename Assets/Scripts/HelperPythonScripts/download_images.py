import os
import json
import requests

def download_image(url, path):
    response = requests.get(url)
    if response.status_code == 200:
        with open(path, 'wb') as file:
            file.write(response.content)

def process_json_file(file_path, output_folder):
    with open(file_path, 'r') as file:
        data = json.load(file)
    
    set_name = os.path.splitext(os.path.basename(file_path))[0]
    small_folder = os.path.join(output_folder, set_name, 'Small')
    large_folder = os.path.join(output_folder, set_name, 'Large')
    
    os.makedirs(small_folder, exist_ok=True)
    os.makedirs(large_folder, exist_ok=True)
    
    for card in data:
        card_id = card['id']
        if 'images' in card:
            if 'small' in card['images']:
                small_image_url = card['images']['small']
                small_image_path = os.path.join(small_folder, f"{card_id}.png")
                download_image(small_image_url, small_image_path)
            
            if 'large' in card['images']:
                large_image_url = card['images']['large']
                large_image_path = os.path.join(large_folder, f"{card_id}-hd.png")
                download_image(large_image_url, large_image_path)

def process_cards_folder(folder_path, output_folder):
    for root, _, files in os.walk(folder_path):
        for file in files:
            if file.endswith('.json'):
                file_path = os.path.join(root, file)
                process_json_file(file_path, output_folder)

cards_folder_path = 'Assets/Data/Cards'
output_folder_path = 'Assets/Data/Cards/CardImages'
process_cards_folder(cards_folder_path, output_folder_path)