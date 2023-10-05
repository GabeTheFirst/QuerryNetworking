# Querry Networking
I an once again making a random online game (called Querry), but I decided instead of just making another server, I would just make some kind of framework for creating servers

There isn't too many features yet and it's all a little Goofy, but here's how to use it:

To start the server you need to be ```using QuerryNetworking.Core;```

Before you run ```Api.StartServer();``` you need to run ```Api.SetupBase();```

```
// it should be something like this
using QuerryNetworking.Core;

internal class Program
{
  static void Main(string[] args)
  {
    Api.SetupBase();
    Api.StartServer();
  }
}

```

then the server should start when you run the app!

to add a request you just need to add the Get Attribute to your function like ```[Get("/api/Goofy")]```, and make sure it returns a string or a byte[]   (for now!)

if you make part of your url {var} like ```[Get("/api/set/{var}/to/{var}")]``` or something, it will pass that many strings into the function, so that function needs to take in two strings
for example:
```
[Get("/api/set/{var}/to/{var}")]
public void ExplainVars(string Var1, string SecondVar)
{
  return "haha you just tried to set " + Var1 + " to " + SecondVar + "!";
}
```
