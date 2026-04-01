# go2web

A command-line HTTP client built on raw TCP sockets for the WEB Lab 5 assignment.

## Features

- **HTTP/HTTPS requests** over raw TCP sockets (no HTTP libraries)
- **DuckDuckGo search** with interactive result selection
- **HTTP redirect** following (301, 302, 303, 307)
- **HTTP caching** with ETag/Last-Modified validation and Cache-Control max-age
- **Content negotiation** — handles both HTML and JSON responses
- **Human-readable output** — strips HTML tags, pretty-prints JSON

## Build & Run

```bash
dotnet build WEB-LAB5/WEB-LAB5.csproj
dotnet run --project WEB-LAB5/WEB-LAB5.csproj -- -u https://example.com
dotnet run --project WEB-LAB5/WEB-LAB5.csproj -- -s "search term"
```

## Usage

### Command Line Options

```
go2web -u <URL>         Make an HTTP request to the specified URL
go2web -s <search-term> Search using DuckDuckGo and show top 10 results
go2web -h               Show help message
```

### Examples

Run from project root using PowerShell or Command Prompt:

#### Example 1: Show Help

```powershell
.\go2web -h
```

**Output:**
```
go2web - Command Line Web Client

Usage:
  go2web -u <URL>         Make an HTTP request to the specified URL
  go2web -s <search-term> Search using DuckDuckGo and show top 10 results
  go2web -h               Show this help message
...
```

---

#### Example 2: Fetch Simple HTML Page

```powershell
.\go2web -u https://example.com
```

**Output:**
```
Fetching: https://example.com
Status: 200 OK

Example Domain

Example Domain

This domain is for use in documentation examples without needing permission. Avoid use in operations.

Learn more
```
*Notice: Clean text output, no HTML tags!*

---

#### Example 3: Fetch JSON API (Content Negotiation)

```powershell
.\go2web -u https://jsonplaceholder.typicode.com/posts/1
```

**Output:**
```
Fetching: https://jsonplaceholder.typicode.com/posts/1
Status: 200 OK

{
  "userId": 1,
  "id": 1,
  "title": "sunt aut facere repellat provident occaecati excepturi optio reprehenderit",
  "body": "quia et suscipit\nsuscipit recusandae consequuntur expedita et cum..."
}
```
*Notice: JSON is automatically formatted with indentation!*

---

#### Example 4: Fetch JSON Array

```powershell
.\go2web -u https://jsonplaceholder.typicode.com/users
```

**Output:**
```
Fetching: https://jsonplaceholder.typicode.com/users
Status: 200 OK

[
  {
    "id": 1,
    "name": "Leanne Graham",
    "username": "Bret",
    "email": "Sincere@april.biz",
    ...
  },
  ...
]
```

---

#### Example 5: Test Cache (Fetch Same URL Twice)

```powershell
# First request - fetches from network
.\go2web -u https://example.com

# Second request - instant from cache
.\go2web -u https://example.com
```

**Output (2nd request):**
```
Fetching: https://example.com
[Retrieved from cache]
Status: 200 OK

Example Domain
...
```
*Notice: "[Retrieved from cache]" message and instant response!*

---

#### Example 6: HTTP Redirect (1 redirect)

```powershell
.\go2web -u http://httpbin.org/redirect/1
```

**Output:**
```
Fetching: http://httpbin.org/redirect/1
Status: 200 OK

{
  "args": {},
  "headers": {
    "Accept": "*/*",
    "Host": "httpbin.org",
    "User-Agent": "go2web/1.0"
  },
  "origin": "...",
  "url": "http://httpbin.org/get"
}
```
*Notice: Automatically followed redirect from `/redirect/1` to `/get`*

---

#### Example 7: HTTP Redirect (5 redirects)

```powershell
.\go2web -u http://httpbin.org/redirect/5
```

**Output:**
```
Fetching: http://httpbin.org/redirect/5
Status: 200 OK

{
  "args": {},
  "headers": {...},
  "url": "http://httpbin.org/get"
}
```
*Notice: Successfully followed 5 redirects!*

---

#### Example 8: HTTP Redirect (10 redirects - max limit)

```powershell
.\go2web -u http://httpbin.org/redirect/10
```

**Output:**
```
Fetching: http://httpbin.org/redirect/10
Status: 200 OK

{
  "args": {},
  "url": "http://httpbin.org/get"
}
```
*Notice: Followed 10 redirects (maximum allowed)*

---

#### Example 9: HTTP Redirect (11 redirects - exceeds limit)

```powershell
.\go2web -u http://httpbin.org/redirect/11
```

**Output:**
```
Fetching: http://httpbin.org/redirect/11
Status: 302 FOUND
```
*Notice: Stops after 10 redirects, returns the 11th redirect response*

---

#### Example 10: Search the Web

```powershell
.\go2web -s "python programming"
```

**Output:**
```
Searching for: python programming

Found 4 results:

[1] Python Tutorial - W3Schools
    https://www.w3schools.com/python/
    W3Schools offers free online tutorials...

[2] The Python Tutorial — Python 3.14.3 documentation
    https://docs.python.org/3/tutorial/index.html
    The Python Tutorial...

[3] Python Tutorial - GeeksforGeeks
    https://www.geeksforgeeks.org/python/python-programming-language-tutorial/
    Python is one of the most popular programming languages...

[4] Python Tutorial
    https://www.tutorialspoint.com/python/index.htm


Enter a number (1-4) to open a result, or press Enter to exit:
```
*Type a number (1-4) to fetch that URL, or press Enter to exit*

---

#### Example 11: Search with Interactive Selection

```powershell
.\go2web -s "web development"
# Then type: 1
```

**Output:**
```
Searching for: web development

Found 5 results:

[1] Web development - MDN Web Docs
    https://developer.mozilla.org/en-US/docs/Learn
    ...

[2] ...

Enter a number (1-5) to open a result, or press Enter to exit:
1

Opening: Web development - MDN Web Docs

Fetching: https://developer.mozilla.org/en-US/docs/Learn
Status: 200 OK

Learn web development
MDN
...
[Full page content displayed]
```
*Notice: Interactive feature - selected result #1 and it was fetched automatically!*

---

#### Example 12: Fetch Plain Text

```powershell
.\go2web -u https://httpbin.org/robots.txt
```

**Output:**
```
Fetching: https://httpbin.org/robots.txt
Status: 200 OK

User-agent: *
Disallow: /deny
```

---

#### Example 13: Fetch Different JSON Endpoint

```powershell
.\go2web -u https://jsonplaceholder.typicode.com/todos/1
```

**Output:**
```
Fetching: https://jsonplaceholder.typicode.com/todos/1
Status: 200 OK

{
  "userId": 1,
  "id": 1,
  "title": "delectus aut autem",
  "completed": false
}
```

---

#### Example 14: Multi-word Search Term

```powershell
.\go2web -s "artificial intelligence machine learning"
```

**Output:**
```
Searching for: artificial intelligence machine learning

Found 6 results:

[1] Artificial Intelligence (AI) - IBM
    ...
```
*Notice: All words are included in the search query*

---

#### Example 15: URL Without Protocol (Auto HTTPS)

```powershell
.\go2web -u example.com
```

**Output:**
```
Fetching: https://example.com
Status: 200 OK

Example Domain
...
```
*Notice: Automatically added "https://" prefix*

---

#### Example 16: Test 404 Error

```powershell
.\go2web -u https://httpbin.org/status/404
```

**Output:**
```
Fetching: https://httpbin.org/status/404
Status: 404 NOT FOUND
```
*Notice: Shows HTTP error status codes correctly*

---

#### Example 17: Fetch JSON Comments

```powershell
.\go2web -u https://jsonplaceholder.typicode.com/comments/1
```

**Output:**
```
Fetching: https://jsonplaceholder.typicode.com/comments/1
Status: 200 OK

{
  "postId": 1,
  "id": 1,
  "name": "id labore ex et quam laborum",
  "email": "Eliseo@gardner.biz",
  "body": "laudantium enim quasi est quidem magnam voluptate..."
}
```

---

#### Example 18: Fetch with Query Parameters

```powershell
.\go2web -u "https://jsonplaceholder.typicode.com/posts?userId=1"
```

**Output:**
```
Fetching: https://jsonplaceholder.typicode.com/posts?userId=1
Status: 200 OK

[
  {
    "userId": 1,
    "id": 1,
    "title": "sunt aut facere...",
    "body": "quia et suscipit..."
  },
  ...
]
```
*Notice: Query parameters are handled correctly*

---

#### Example 19: HTTP Methods Endpoint

```powershell
.\go2web -u https://httpbin.org/get
```

**Output:**
```
Fetching: https://httpbin.org/get
Status: 200 OK

{
  "args": {},
  "headers": {
    "Accept": "*/*",
    "Host": "httpbin.org",
    "User-Agent": "go2web/1.0"
  },
  "origin": "xxx.xxx.xxx.xxx",
  "url": "https://httpbin.org/get"
}
```
*Notice: Shows request headers that were sent*

---

#### Example 20: Test Response Headers

```powershell
.\go2web -u https://httpbin.org/response-headers?Custom-Header=test
```

**Output:**
```
Fetching: https://httpbin.org/response-headers?Custom-Header=test
Status: 200 OK

{
  "Content-Length": "...",
  "Content-Type": "application/json",
  "Custom-Header": "test"
}
```

---

#### Example 21: Different Search Query

```powershell
.\go2web -s "C# programming tutorial"
```

**Output:**
```
Searching for: C# programming tutorial

Found 5 results:

[1] C# Tutorial - W3Schools
    https://www.w3schools.com/cs/
    ...

[2] C# documentation - Microsoft Learn
    https://learn.microsoft.com/en-us/dotnet/csharp/
    ...

Enter a number (1-5) to open a result, or press Enter to exit:
```

---

#### Example 22: Absolute Redirect URL

```powershell
.\go2web -u http://httpbin.org/absolute-redirect/2
```

**Output:**
```
Fetching: http://httpbin.org/absolute-redirect/2
Status: 200 OK

{
  "args": {},
  "url": "http://httpbin.org/get"
}
```
*Notice: Successfully followed absolute redirect URLs*

---

#### Example 23: Relative Redirect URL

```powershell
.\go2web -u http://httpbin.org/relative-redirect/2
```

**Output:**
```
Fetching: http://httpbin.org/relative-redirect/2
Status: 200 OK

{
  "args": {},
  "url": "http://httpbin.org/get"
}
```
*Notice: Successfully resolved and followed relative redirect URLs*

---

#### Example 24: Large JSON Response

```powershell
.\go2web -u https://jsonplaceholder.typicode.com/photos
```

**Output:**
```
Fetching: https://jsonplaceholder.typicode.com/photos
Status: 200 OK

[
  {
    "albumId": 1,
    "id": 1,
    "title": "accusamus beatae ad facilis...",
    "url": "https://via.placeholder.com/600/92c952",
    "thumbnailUrl": "https://via.placeholder.com/150/92c952"
  },
  ...
]

... (truncated, 45000 more characters)
```
*Notice: Large responses are automatically truncated for readability*

---

#### Example 25: Cache Expiration Test

```powershell
# Fetch once
.\go2web -u https://httpbin.org/cache/3600

# Fetch again immediately (cached for 1 hour)
.\go2web -u https://httpbin.org/cache/3600
```

**Output (2nd request):**
```
Fetching: https://httpbin.org/cache/3600
[Retrieved from cache]
Status: 200 OK
...
```
*Notice: Cache respects Cache-Control: max-age=3600 header*
