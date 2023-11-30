# CancellationTokenDemo
## Running
- Run it with your favorite IDE or just by running `dotnet run`
- Swagger URL will be available from https://localhost:5001
- Open up `test.html` to check on the javascript sample. Make sure to unlock CORS from your browser

## Disclaimer
This is a demonstration project and its only goal is to walk around alternative implementations for `CancellationToken` in C#. 
I'm using a webapi project as this sample  but it can and should be used to any application that deals with IO and asynchronous operations.
Check on references for more info about the subjects hereby covered.

## References
- [Cancellation in managed threads](https://learn.microsoft.com/en-us/dotnet/standard/threading/cancellation-in-managed-threads)
- [Recommended Patterns for CancellationToken](https://devblogs.microsoft.com/premier-developer/recommended-patterns-for-cancellationtoken/)