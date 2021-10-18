# Qvoid-Authentication
Advanced authentication system, can be used by everyone for free!

Stability: Experimental ⤵️ <br><br>
[![Stability: Experimental](https://masterminds.github.io/stability/experimental.svg)](https://masterminds.github.io/stability/experimental.html)

# Some information:
I made this authentication system for one of my programs, I decided to release it as a library (Converting everything to user-friendly library); **this project is undone yet!**

If you have found some bug that you would like to be fixed or you just want to give a suggestion, open an issue on the Issues tab.

I think this library is better than lots of the paid authentication systems because FireBase is pretty fast and I tried to keep the code as much optimized as I can.

Have fun 😝

# Features:
* Supports Discord webhook integration.
* Auto-update system.
* Killswitch.
* Blacklist system.
* Some state functions.
* HWID Lock.
* Encryption system.
* Licenses with specific duration.
* And some general functions such as Login, Register, Create License.

# Plans:
* Make a Documentation.
* Upload the auto-update system (it's already ready but I'm lazy to upload).

# Getting started:
If you don't have a FireBase account you will need to create one.

It will be much easier learning the FireBase interface by yourself or watching some tutorials on FireBase. 

After you have a FireBase account and you are connected, create a project and then create a "Realtime Database".
Go to Project Settings -> Service Account -> Database Secrets and copy the key that is listed there.

```cs
//Creating the credentials
Credentials credentials = new Credentials()
{
    //The current version of this distribution, Should look like: "0.0.0"
    Version = Version.Parse("1.0.0"),
    
    //The base address of the database, Should look like (Depends on the country you have chose): "https://test-b572d-default-rtdb.europe-west1.firebasedatabase.app/"
    BaseAddress = "",
    
    //The database secret you have copied before, Should look like: "4RLlgzCxDQ62JmJMzfFpLFruqKfPHmZUFJq4rya4"
    Token = ""
};

AuthSystem auth = new AuthSystem(credentials);

//Now you can use the functions of the auth system, I will create short a documentation later.
```

# LEGAL DISCLAIMER

The author does not hold any responsibility for the bad use of this tool, remember that attacking targets without prior consent is illegal and punished by law.
