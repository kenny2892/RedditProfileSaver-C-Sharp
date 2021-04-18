import praw
import time

def convert_reddit_gallery(url, reddit):
    new_url = url
    image_links = convert_reddit_gallery_to_urls(url, reddit)
    
    if(image_links is not None or len(image_links) != 0):
        for index in image_links:
            new_url = new_url + " " + image_links[index]
        
    return new_url
    
def convert_reddit_gallery_to_urls(url, reddit):
    image_links = {}
    submission = reddit.submission(url=url)
    
    if(submission is None):
        image_links = {}
        return image_links
    
    try:
        i = 0
        for item in sorted(submission.gallery_data['items'], key=lambda x: x['id']):
            media_id = item['media_id']
            meta = submission.media_metadata[media_id]

            if meta is None or meta['status'] == 'failed':
                continue
            
            elif meta['e'] == 'Image':
                source = meta['s']
                image_links[i] = '%s' % (source['u'])
                i = i + 1
                
            elif meta['e'] == 'AnimatedImage':
                source = meta['s']
                image_links[i] = '%s' % (source['gif'])
                i = i + 1
            
            # Sleep for 1 Sec
            time.sleep(1)
        
    except:
        image_links = {}
        
    return image_links
