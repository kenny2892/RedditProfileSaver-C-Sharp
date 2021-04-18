import time
import requests

def convert_imgur_gallery(url, imgur_id):
    # Sleep for 1 Sec
    time.sleep(1)

    id = ""
    new_rul = url
    if("imgur.com/a/" in new_rul):
        index = new_rul.index("/a/") + 3
        id = new_rul[index:]

    else:
        return new_rul

    client_id = "Client-ID " + imgur_id
    response = requests.get(
        "https://api.imgur.com/3/album/" + id + "/images",
        headers={"Authorization": client_id},
    )

    json_response = response.json()
    success = json_response["success"]
    status = json_response["status"]

    if(success and status == 200):
        data = json_response["data"]

        for item in data:
            new_rul = new_rul + " " + item["link"]

    return new_rul
