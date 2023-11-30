using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CancellationTokenDemo.Controllers;

[ApiController]
[Route("[controller]")]
public class SlowController(ILogger logger) : ControllerBase
{
    /// <summary>
    /// Example of a method that takes 10s to run and does not have implemented CancellationToken pattern
    /// </summary>
    /// <returns>A `string` value 'ok!' if it got the the end without cancellation being triggered
    /// or a `string` value 'nok' if it got cancelled before finishing the method execution</returns>
    [HttpGet]
    public async Task<string> SlowMethod(){
        try{
            logger.LogInformation("processing data...");
            await Task.Delay(10000);
            logger.LogInformation("ok!");
            return "ok!";
        }
        catch(Exception ex)
        {
            //This will never happen, because we don't have a Cancellation Token
            logger.LogError($"Slow method response: {ex.Message}");
            return "nok";
        }
    }

    /// <summary>
    /// Example of a method that takes 10s to run and respects CancellationToken pattern
    /// If the browser abandons the request, it'll be propagated downstream through `cancellationToken` parameter
    /// </summary>
    /// <returns>A `string` value 'ok!' if it got the the end without cancellation being triggered
    /// or a `string` value 'nok' if it got cancelled before finishing the method execution</returns>
    [HttpGet("Fixed")]
    public async Task<string> FixedSlowMethod(CancellationToken cancellationToken){
        try{
            logger.LogInformation("processing data...");
            await Task.Delay(10000, cancellationToken);
            logger.LogInformation("ok!");
            return "ok!";
        }
        catch(OperationCanceledException ex){
            //If the browser is abandoned, page is changed, timeout is raised, or anything else goes worng with the caller, this will happen
            logger.LogError($"Slow method response: {ex.Message}");
            //This is where you can rollback, dispose or do anything to handle a cancellation
            logger.LogError($"If needed, I can rollback anything from here");
            return "nok";
        }catch{
            return "nok";
        }
    }
}