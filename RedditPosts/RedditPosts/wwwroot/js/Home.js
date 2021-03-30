function HomeSetup() // Source for using method: https://stackoverflow.com/a/25605074
{
    var count = 1;
    var words = "Retrieving Upvotes";
    var textArea = document.getElementById("loadingMsg");
    var upvoteDisplay = document.getElementById("upvoteCounter");
    var iconDisplay = document.getElementById("iconRetrieving");
    var isRetrieving = false;

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
        if(isRetrieving)
        {
            console.log("Button not active");
            return;
        }

        isRetrieving = true;
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
                        for(var i = 0; i < count; i++)
                        {
                            textArea.textContent += ".";
                        }

                        count++;

                        if(count == 4)
                        {
                            count = 0;
                        }
                    }

                    else
                    {
                        textArea.textContent = "";
                        upvoteDisplay.textContent = "";
                        isRetrieving = false;

                        $.ajax // Add new posts to the database
                        ({
                            url: "/Home/UpdatePosts",
                            data: {}
                        });

                        clearInterval(interval);
                    }
                },
            });
        }, 1000);
    }

    function updateIcons()
    {
        iconDisplay.textContent = "Updating Icons";

        $.ajax // Launch the retrieval Python Script
            ({
                url: "/Home/UpdateIcons",
                data: {}
            })
            .done(function(result) 
            {
                if(result) 
                {
                    iconDisplay.textContent = "";
                }

                else
                {
                    iconDisplay.textContent = "FAILED";
                }
            })
            .fail(function(xhr, ajaxOptions, thrownError) 
            {
                iconDisplay.textContent = "FAILED";
                console.log("Error in updateIcons:", thrownError);
            });
    }
}