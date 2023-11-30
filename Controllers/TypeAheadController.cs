using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CancellationTokenDemo.Controllers;

[ApiController]
[Route("[controller]")]
public class TypeAheadController(ILogger logger) : ControllerBase
{
    /// <summary>
    /// TypeAhead implementation sample with CancellationToken pattern.
    /// Whenever the browser cancels a request in the execution (see test.html) the cancellation is replicated through `ct` variable
    /// </summary>
    /// <param name="param">Sample param for logging examples</param>
    /// <param name="ct">CancellationToken to be propagated</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IEnumerable<string>> Get(string param, CancellationToken ct)
    {
        try
        {
            logger.LogInformation($"Searching for '{param}'...");
            //Cancellation point 1
            var text = await System.IO.File.ReadAllTextAsync("names.json", ct);

            var jsonNames = JsonConvert.DeserializeObject<List<string>>(text);
            var items = jsonNames.Where(x => string.IsNullOrEmpty(param) || x.ToLower().StartsWith(param.ToLower()))
                .ToList();

            var delay = Math.Min(items.Count / 3, 3); //For demo purposes, maximum delay will be 4s

            //Cancellation point 2
            // _logger.LogInformation($"Waiting server query {param}... {delay + 1}s");
            // await Task.Delay(TimeSpan.FromSeconds(1), ct);
            ct.ThrowIfCancellationRequested();

            //Non-returning point (committed to the transaction)
            // _logger.LogInformation("Non-returning point reached");
            await Task.Delay(TimeSpan.FromSeconds(delay), CancellationToken.None);
            logger.LogInformation($"Returning {param} to API");
            return items;
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError($"Cancelled! {param}\n{ex.Message}");
        }

        return null;
    }
}