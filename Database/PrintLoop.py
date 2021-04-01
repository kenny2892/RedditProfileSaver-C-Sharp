import pathlib
from time import sleep
from RepeatedTimer import *

def printToFile():
    text = "\nPost 1 has been stored."

    file_path = str(pathlib.Path(__file__).parent.absolute()) + "/Results.txt"
    resultsFile = open(file_path, "a")
    
    resultsFile.write(text)
    resultsFile.close()
    
    print(text)
    
def main():
    rt = RepeatedTimer(1, printToFile) # it auto-starts, no need of rt.start()

    try:
        sleep(10) # your long-running job goes here...
    finally:
        rt.stop() # better in a try/finally block to make sure the program ends!

    file_path = str(pathlib.Path(__file__).parent.absolute()) + "/Results.txt"
    resultsFile = open(file_path, "a")
    
    resultsFile.write("\nCompleted")
    resultsFile.close()

main()