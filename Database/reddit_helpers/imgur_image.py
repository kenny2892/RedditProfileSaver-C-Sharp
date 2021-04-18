import time
import requests

def convert_imgur_image(url, imgur_id):
    # Sleep for 1 Sec
    time.sleep(1)

    id = ""
    new_url = url
    if("//imgur.com/" in new_url):
        index = new_url.index("com/") + 4
        id = new_url[index:]

    else:
        return new_url

    client_id = "Client-ID " + imgur_id
    response = requests.get(
        "https://api.imgur.com/3/image/" + id,
        headers={"Authorization": client_id},
    )

    json_response = response.json()
    success = json_response["success"]
    status = json_response["status"]

    if(success and status == 200):
        data = json_response["data"]

        for item in data:
            new_url = new_url + " " + item["link"]

    return new_url
