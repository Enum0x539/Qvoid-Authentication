# Qvoid-Authentication
🔐 "Who needs passwords when you have HWID lock on speed dial?"

[![Stability: Experimental](https://masterminds.github.io/stability/experimental.svg)](https://masterminds.github.io/stability/experimental.html)

Qvoid-Authentication is an advanced, developer-friendly authentication system designed for C# applications — completely free and extensible.
Built with performance in mind and powered by Firebase, this project aims to simplify secure user management, license control, and client communication.

# 🧠 About the Project

Originally created for internal use in one of my personal tools, this system evolved into a general-purpose library that anyone can use and contribute to. While still experimental, it already offers features that rival many paid alternatives — without locking you behind a paywall.

🛠️ `“It works on my machine”` — me, after testing in debug mode once (jk!).
    
💥 Features

    ✅ Login, Register, Create License

    🎟️ Licensing system with expiration support

    🛑 Kill switch for remote disable

    🚫 HWID lock (because passwords are so last decade)

    ❌ Blacklist system

    🔐 Simple encryption

    📡 Discord webhook integration

    🔄 Auto-update system (lazy dev note: not pushed yet 😅)

    📊 User state tracking (basic telemetry)
    
📦 Setup Instructions
🧬 Prerequisites

    A Firebase account.

    A working Realtime Database in Firebase.

🔧 Firebase Setup

    Create a new Firebase project.

    Enable the Realtime Database.

    Navigate to Project Settings → Service Accounts → Database Secrets and copy your key.

🚀 Getting Started
```csharp
// Step 1: Set up your credentials
Credentials credentials = new Credentials()
{
    Version = Version.Parse("1.0.0"), // your app version
    BaseAddress = "", // e.g. "https://your-app-id.firebaseio.com/"
    Token = "" // your Firebase DB secret
};

// Step 2: Initialize the authentication system
AuthSystem auth = new AuthSystem(credentials);

// You can now call auth.Login(), auth.Register(), auth.CreateLicense(), etc.
```

🗺️ Roadmap

🧾 Documentation

📤 Push auto-update module

🧪 More unit tests

🎉 Maybe add a dashboard frontend?

# LEGAL DISCLAIMER

The author does not hold any responsibility for the bad use of this tool, remember that attacking targets without prior consent is illegal and punished by law.
