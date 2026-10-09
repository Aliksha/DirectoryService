import json
import uuid
from datetime import datetime
import random

# Список строго тематических английских названий для локаций
NAME_PLACES = [
    "Main HQ",
    "Dubbing Studio",
    "Belarusian Branch",
    "Sound Production Lab",
    "Subtitles Office",
    "Translation Center",
    "Media Storage",
    "Mao Mao Cafe Vitebsk",
    "Movement Center"
]

# Список английских названий улиц города Витебска для разнообразия адресов
STREETS = [
    "Polar Street", "Peach Street", "Apple Prospekt", "Strowberry Street", 
    "Dandelion Avenue", "Rose Street", "Cake Street", "Bread Street", 
    "Summer Street", "Pink Street", "Sunny Street", "Cloudy Street"
]

LOCATION_GEOGRAPHY = [
    {"city": "Moscow", "country": "Russia"},
    {"city": "Berlin", "country": "Germany"},
    {"city": "London", "country": "UK"},
    {"city": "Bucharest", "country": "Romania"},
    {"city": "Warsaw", "country": "Poland"},
    {"city": "Copenhagen", "country": "Denmark"},
    {"city": "Vitebsk", "country": "Belarus"}
]

TIME_ZONES = ["MSK", "EET", "MST", "UTC+3"]

def generate_sql_file(filename="insert_locations.sql", count=10000):
    with open(filename, "w", encoding="utf-8") as f:
        f.write("-- Automated SQL script for test data generation (Standalone Mode)\n")
        f.write("-- Target table: locations\n\n")
        
        for i in range(count):
            loc_id = str(uuid.uuid4())
            is_active = "true" if random.random() > 0.1 else "false" # 90% активных
            now_str = datetime.now().strftime("%Y-%m-%d %H:%M:%S.%f+03")
            
             geo = random.choice(LOCATION_GEOGRAPHY)

            # Генерируем имя: если индекс выходит за рамки списка, делаем нумерованный филиал
            if i < len(NAME_PLACES):
                name = f"{NAME_PLACES[i]} ({geo['city']})"
            else:
                base_name = random.choice(NAME_PLACES)
                name = f"{base_name} ({geo['city']}) #{i + 1}"
                
            time_zone = random.choice(TIME_ZONES)
            
            # Структура адреса JSONB на английском
            address_dict = {
                "city": geo["city"],
                "street": random.choice(STREETS),
                "country": geo["country"],
                "house_number": str(random.randint(1, 150))
            }
            # Переводим в JSON и экранируем кавычки для SQL
            address_json = json.dumps(address_dict, ensure_ascii=False).replace("'", "''")
            
            sql_line = (
                f"INSERT INTO locations (id, is_active, created_at, updated_at, name, time_zone, address, \"DeletedAt\", \"SoftDeleted\") "
                f"VALUES ('{loc_id}', {is_active}, '{now_str}', '{now_str}', '{name}', '{time_zone}', '{address_json}', NULL, false);\n"
            )
            f.write(sql_line)
            
    print(f"🎉 File '{filename}' successfully created! Generated {count} clean rows.")

if __name__ == "__main__":
    import os
    # Автоматически определяем путь к Рабочему столу
    desktop_path = os.path.join(os.path.expanduser("~"), "Desktop", "insert_locations.sql")
    
    # Запускаем генерацию по этому пути
    generate_sql_file(filename=desktop_path, count=10000)

