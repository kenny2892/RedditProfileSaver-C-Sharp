function LoadSubreddits(params)
{
    var subredditGrid = document.getElementById("subredditGrid");
    var loadingSubredditsDisplay = document.getElementById("loadingSubreddits");

    $.ajax
    ({
        type: 'POST',
        url: "/Subreddits/_Subreddits",
        data: params,
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