function LoadSubreddits()
{
    var subredditGrid = document.getElementById("subredditGrid");
    var loadingSubredditsDisplay = document.getElementById("loadingSubreddits");

    $.ajax
    ({
        url: "/Subreddits/_Subreddits",
        dataType: "html"
    })
    .done(function(result) 
    {
        if(result) 
        {
            subredditGrid.innerHTML += result;
        }
    })
    .fail(function(xhr, ajaxOptions, thrownError) 
    {
        console.log("Error in LoadSubreddits:", thrownError);
    })
    .always(function() 
    {
        loadingSubredditsDisplay.textContent = "";
    });
}