function HomeSetup() // Source for using method: https://stackoverflow.com/a/25605074
{
    var upvoteCounter = 1;
    var words = "Retrieving Upvotes";
    var textArea = document.getElementById("loadingMsg");
    var upvoteDisplay = document.getElementById("upvoteCounter");
    var iconDisplay = document.getElementById("iconRetrieving");
    var isRetrievingUpvotes = false;
    var isRetrievingSubreddits = false;

    var retrieveUpvotesBtn = document.getElementById("retrieveUpvoteBtn");
    var updateIconsBtn = document.getElementById("updateIconsBtn");

    retrieveUpvotesBtn.onclick = function()
    {
        retrieveUpvotes();
    };

    updateIconsBtn.onclick = function()
    {
        updateIcons();
    };

    function retrieveUpvotes()
    {
        if(isRetrievingUpvotes)
        {
            console.log("Button not active");
            return;
        }

        isRetrievingUpvotes = true;
        $.ajax // Launch the retrieval Python Script
        ({
            url: "/Home/Retrieve",
            data: {}
        });

        const interval = setInterval(function()
        {
            $.ajax
            ({
                url: "/Home/RetrieveUpvoteCount",
                data: {},
                success: function(data, textStatus, jqXHR) // Data is the return value
                {
                    upvoteDisplay.textContent = "Posts Retrieved: " + data;
                },
            });

            $.ajax
            ({
                url: "/Home/isRetrievingUpvotes",
                data: {},
                success: function(data, textStatus, jqXHR) // Data is the return value
                {
                    // console.log("Recieved");
                    if(data)
                    {
                        textArea.textContent = words;
                        for(var i = 0; i < upvoteCounter; i++)
                        {
                            textArea.textContent += ".";
                        }

                        upvoteCounter++;

                        if(upvoteCounter == 4)
                        {
                            upvoteCounter = 0;
                        }
                    }

                    else
                    {
                        setTimeout(() => { textArea.textContent = ""; upvoteDisplay.textContent = ""; }, 10); // Need to have it delayed to properly display
                        isRetrievingUpvotes = false;
                        clearInterval(interval);
                    }
                },
            });
        }, 1000);
    }

    function updateIcons()
    {
        if(isRetrievingSubreddits)
        {
            console.log("Button not active");
            return;
        }

        var subredditCounter = 1;
        var defaultTxt = "Updating Subreddits";
        isRetrievingSubreddits = true;
        iconDisplay.textContent = "Updating Icons";

        $.ajax // Launch the start method
        ({
            url: "/Home/StartUpdateSubredditIcons",
            data: {}
        })
        .fail(function(xhr, ajaxOptions, thrownError) 
        {
            iconDisplay.textContent = "FAILED";
            console.log("Error in updateIcons:", thrownError);
            return;
        });

        const interval = setInterval(function()
        {
            $.ajax
            ({
                url: "/Home/IsGettingSubreddits",
                data: {},
                success: function(data)
                {
                    if(data)
                    {
                        iconDisplay.textContent = defaultTxt;
                        for(var i = 0; i < subredditCounter; i++)
                        {
                            iconDisplay.textContent += ".";
                        }

                        subredditCounter++;

                        if(subredditCounter == 4)
                        {
                            subredditCounter = 0;
                        }
                    }

                    else
                    {
                        iconDisplay.textContent = "Applying Updates";
                        $.ajax // Apply the updated subreddits
                        ({
                            url: "/Home/ApplyUpdatedSubreddits",
                            data: {}
                        })
                        .done(function() 
                        {
                            setTimeout(() => {iconDisplay.textContent = "";}, 10); // Need to have it delayed to properly display
                            isRetrievingSubreddits = false;
                            clearInterval(interval);
                        });
                    }
                },
            });
        }, 1000);
    }
}