import pathlib
import sys
from time import sleep
from RepeatedTimer import * # pylint: disable=unused-wildcard-import

count = 0

def printToFile():
    global count
    count = count + 1
    text = "Post " + str(count) + " has been stored."

    print(text)
    sys.stdout.flush()

    # file_path = str(pathlib.Path(__file__).parent.absolute()) + "/Results.txt"
    # resultsFile = open(file_path, "a")
    
    # resultsFile.write(text)
    # resultsFile.close()
    
    # print(text)
    
def main():
    rt = RepeatedTimer(0.5, printToFile) # it auto-starts, no need of rt.start()

    try:
        while(count < 20):
            continue
    finally:
        rt.stop() # better in a try/finally block to make sure the program ends!

    # file_path = str(pathlib.Path(__file__).parent.absolute()) + "/Results.txt"
    # resultsFile = open(file_path, "a")
    
    # resultsFile.write("\nCompleted")
    # resultsFile.close()

    print("Completed")
    sys.stdout.flush()

main()