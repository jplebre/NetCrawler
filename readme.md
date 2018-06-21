# NetCrawler

Crawls `website of choice`  
Made some decisions on what to crawl, eg.:  
- Decided to ignore cloudflare files, to ignore links to elements in page etc.  
- Decided to only keep one level deep linking. It'll print every page and every link for each of those pages, but only one level down.  

If it was a production feature I'd be bouncing these ideas with team and product manager what we should do for those scenarios.  

## How To Run
You need dotnet core 2.1 available [here](https://www.microsoft.com/net/download/windows)  
Preferably run the app on windows as there may be some quirks with the HttpClient (namely SSL handling)

on the CLI, navigate to the app dir (the actual project folder) and run the following commands:  
`dotnet restore`  
`dotnet build`  
`dotnet run`  

The output will be generated on: the bin folder, under a file called `websitemap_*timenow*.txt`

## Things I'd like to add with more time:
### Wrap console as if it was an actual logger framework  
Create an `ILogger` interface that wraps the console. I could then add tests or verify loggin happened as part of a test (eg. for crucial error handling)  

### Add IoC to generated and manage all the dependencies
It would clean up that `var crawler = new Crawler(new WebPageParser(new RestClient(new HttpClient())));` nonsense, whilst keeping Dependency Injection going for easier testability.  
It would also allow me to very easily hook up IOptions, which are a strongly typed configuration access object, where I could grab values such as httpcall timeout and concurrency without hardcoding everywhere.  

### Add console parsing ability
would be nice users could do something like `dotnet run -host https://www.wikipedia.com -maxconcurrency 10 -timelimit 120` or similar.  
There's a well known library for this in C#, probably could implement something in a few minutes. Specially with IOC already hooked up  

### Add settings to be pulled via configuration 

### Instead of using a for loop for retry logic, I'd probably use a library like Polly instead
Don't reinvent the wheel! there's great OSS projects out there with tons of contributors. I wouldn't be able to catch up with them :)

### Refactor so multiple levels of depth can be achieved
Eg.: if someone wants the website map to go several layers deep

## Stats
__taken on a 2015 macbook pro quad core runing windows on bootcamp__

How many concurrent pages?                                | TimeTaken     | No. of Pages  
--------------------------------------------------------- | ------------- | -------------  
**No parallelization:**                                   | 3min33s       |           557  
**Asynchronously doing up to 2 calls concurrently:**      | 5min01s       |           557  
**Asynchronously doing up to 4 calls concurrently:**      | 1min54s       |           557  
**Asynchronously doing up to 6 calls concurrently:**      | 1min44s       |           557  
**Asynchronously doing up to 10 calls concurrently:**     | 1min32s       |           557