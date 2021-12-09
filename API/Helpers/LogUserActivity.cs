﻿using API.Extension;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Helpers;

public class LogUserActivity : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // context after action was executed
        var resultContext = await next();

        if (resultContext.HttpContext.User.Identity is {IsAuthenticated: false}) return;

        var claimId = resultContext.HttpContext.User.GetUserId();
        if (claimId is null) return;
        var userId = claimId.Value;
        
        var repo = resultContext.HttpContext.RequestServices.GetService<IUserRepository>();
        var user = await repo!.GetUserByIdAsync(userId);
        user.LastActive = DateTime.Now;
        await repo.SaveAllAsync();
    }
}