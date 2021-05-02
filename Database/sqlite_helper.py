import sqlite3
from sqlite3 import Error
# Source for how to use Sqlite in Python: https://www.sqlitetutorial.net/sqlite-python/

def create_connection(db_file):
    """ create a database connection to the SQLite database
        specified by db_file
    :param db_file: database file
    :return: Connection object or None
    """
    conn = None
    try:
        conn = sqlite3.connect(db_file)
    except Error as e:
        print(e)

    return conn

def create_post(conn, reddit_posts):
    """
    Create a new project into the projects table
    :param conn:
    :param reddit_posts:
    :return: reddit_posts id
    """
    sql = ''' INSERT INTO RedditPost(Number, Title, Author, Subreddit, Hidden, Date, UrlContent, UrlPost, UrlThumbnail, IsSaved, IsNsfw)
              VALUES(?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?) '''
    cur = conn.cursor()
    cur.execute(sql, reddit_posts)
    conn.commit()
    return cur.lastrowid

def find_post_count(conn):
    cur = conn.cursor()
    cur.execute("SELECT * FROM sqlite_sequence")
    rows = cur.fetchone()

    return int(rows[1])

def select_all_post_keys(conn):
    """
    Query all rows in the RedditPosts table
    :param conn: the Connection object
    :return:
    """
    cur = conn.cursor()
    cur.execute("SELECT * FROM RedditPost ORDER BY NUMBER DESC")

    rows = cur.fetchall()

    post_keys = []
    for row in rows:
        post_keys.append(row[2] + " - " + row[3] + " - " + row[6])

    return post_keys