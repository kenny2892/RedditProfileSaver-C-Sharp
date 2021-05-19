import time
import requests

def convert_redgif(url):
    html = requests.get(url).text

    start_index = html.index("og:video\" content=\"") + 19
    html = html[start_index:]
    
    end_index = html.index("\"")
    new_url = html[:end_index]
    new_url = new_url.replace("-mobile.mp4", ".mp4")
    
    # Sleep for 1 Sec
    time.sleep(1)

    return new_url
