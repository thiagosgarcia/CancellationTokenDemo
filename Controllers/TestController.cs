using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CancellationTokenDemo.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CancellationTokenDemo.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController(ILogger logger) : ControllerBase
{
    /// <summary>
    /// SyncMethod without cancellation token
    /// If you refresh the browser during execution you'll notice the Cancellation isn't propagated
    /// </summary>
    [HttpGet("SyncMethod")]
    public void SyncMethod(){
        for (int i = 0; i < 10; i++)
        {
            Thread.Sleep(1000);
            logger.LogInformation($"{i + 1} seconds passed...");
        }
        logger.LogInformation($"Finished. Returning...");
    }

    /// <summary>
    /// AsyncMethod without cancellation token
    /// If you refresh the browser during execution you'll notice the Cancellation isn't propagated
    /// </summary>
    [HttpGet("AsyncMethod")]
    public async Task AsyncMethod(){
        for (int i = 0; i < 10; i++)
        {
            await Task.Delay(1000);
            logger.LogInformation($"{i + 1} seconds passed...");
        }
        logger.LogInformation($"Finished. Returning...");
    }

    /// <summary>
    /// Polling cancelling pattern
    /// During a long running execution `cancellationToken.ThrowIfCancellationRequested()` is called every second so whenever
    /// the request is abandoned, it'll stop the method right away.
    /// </summary>
    /// <param name="cancellationToken">CancellationToken reference object</param>
    [HttpGet("Example1")]
    public async Task Example1(CancellationToken cancellationToken){
        for (int i = 0; i < 10; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(1000);
            logger.LogInformation($"{i + 1} seconds passed...");
        }
        logger.LogInformation($"Finished. Returning...");
    }

    /// <summary>
    ///
    /// Polling cancelling pattern
    /// During a long running execution `cancellationToken.ThrowIfCancellationRequested()` is called every second so whenever
    /// the request is abandoned, it'll stop the method right away.
    /// </summary>
    /// <param name="param1">Sample parameter for logging purposes</param>
    /// <param name="param2">Sample parameter for setting how many seconds the method  will run before returning successfully</param>
    /// <param name="cancellationToken">CancellationToken reference object</param>
    [HttpGet("Example2/{param1}")]
    public async Task Example2(string param1, [FromQuery]int param2 = 10, CancellationToken cancellationToken = default){
        for (int i = 0; i < param2; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            //Additionally you can pass cancellation token to any method that accepts it, so it's better propagated
            //instead of jus wait for the polling verifications
            await Task.Delay(1000, cancellationToken);
            logger.LogInformation($"{i + 1} seconds passed... Params: {param1}");
        }
        logger.LogInformation($"Finished. Returning...");
    }

    /// <summary>
    /// Propagating cancellation token to downstream services
    /// When dealing with other IO services, you should propagate so it also fails fast.
    /// </summary>
    /// <param name="cancellationToken">CancellationToken reference object</param>
    /// <returns>A string value</returns>
    [HttpGet("Example3")]
    public async Task<string> Example3(CancellationToken cancellationToken){
            
        var client = new HttpClient();
        logger.LogInformation("Calling a slow service...");
        var result = await client.GetStringAsync("https://localhost:5001/Slow/", cancellationToken);
        return result;
    }

    /// <summary>
    /// Propagating cancellation token to downstream services
    /// When dealing with other IO services, you should propagate so it also fails fast.
    /// </summary>
    /// <param name="cancellationToken">CancellationToken reference object</param>
    /// <returns>A string value</returns>
    [HttpGet("Example4")]
    public async Task<string> Example4(CancellationToken cancellationToken){
            
        var client = new HttpClient();
        logger.LogInformation("Calling a slow service...");
        var result = await client.GetStringAsync("https://localhost:5001/Slow/Fixed", cancellationToken);
        return result;
    }

    /// <summary>
    /// Propagating cancellation token to downstream services
    /// When dealing with other IO services, you should propagate so it also fails fast.
    /// </summary>
    /// <param name="cancellationToken">CancellationToken reference object</param>
    /// <returns>A string value</returns>
    [HttpGet("Example5")]
    public Task<string> Example5(CancellationToken cancellationToken){

        var client = new HttpClient();
        logger.LogInformation("Calling an external service...");

        //Passing through the Cancellation Token: Every service downstream will be notified if they also implement Cancellation Token pattern
        return client.GetStringAsync("https://localhost:5001/Slow", cancellationToken);
    }

    /// <summary>
    /// Propagating cancellation token to downstream services and handling with method attribute
    /// Adding a LinkedTokenSource helps to link multiple CancellationTokens into one and propagate any cancellation across them all
    /// </summary>
    /// <param name="cancellationToken">CancellationToken reference object</param>
    /// <returns>A string value</returns>
    [HandleTimeout]
    [HttpGet("TimeoutPropagation")]
    public async Task<string> TimeoutPropagation(CancellationToken cancellationToken){ //Inherited CT
            
        var client = new HttpClient();
        logger.LogInformation("Calling a slow service...");
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(6)); //Timeout CT
        var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken); //Linking both CTs
        var resultTask = client.GetStringAsync("https://localhost:5001/Slow/Fixed", linkedToken.Token);

        return await resultTask;
    }

    /// <summary>
    /// Propagating cancellation token to downstream services and handling with method attribute
    /// Adding a LinkedTokenSource helps to link multiple CancellationTokens into one and propagate any cancellation across them all
    /// This sample forces the cancelling if method takes longer than 4s to run
    /// </summary>
    /// <param name="cancellationToken">CancellationToken reference object</param>
    /// <returns>A string value</returns>
    [HandleTimeout]
    [HttpGet("ExceptionHandlerAndTimeoutPropagation")]
    public async Task<string> ExceptionHandlerAndTimeoutPropagation(CancellationToken cancellationToken){ //Inherited CT
            
        var client = new HttpClient();
        logger.LogInformation("Calling a slow service...");
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(6)); //Timeout CT
        var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken); //Linking both CTs
        var resultTask = client.GetStringAsync("https://localhost:5001/Slow/Fixed", linkedToken.Token);

        if(!Task.WaitAll(new Task[]{ resultTask }, 4000, linkedToken.Token)) //Example of how to cancel both tokens regardless other conditions
        {
            logger.LogInformation("Timeout!");
            await linkedToken.CancelAsync();
        }

        return await resultTask;
    }
}