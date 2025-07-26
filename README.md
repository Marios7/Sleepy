# Sleppy 💤

**Sleppy** is a lightweight windows only utility that prevents your PC from going to sleep due to inactivity. It was built to help remote workers stay connected and avoid disruptions caused by strict system sleep policies enforced by corporate IT departments.

## 🧠 Why Sleppy?

Many companies enforce aggressive sleep settings (e.g., 3 minutes of inactivity) for security or energy-saving reasons. While that's reasonable in shared offices or public spaces, it's frustrating for remote workers who:

- Run long tasks or scripts that don’t interact with the UI
- Attend long video meetings without frequent keyboard/mouse input
- Need to appear "Available" on collaboration tools like Microsoft Teams

Sleppy keeps your machine awake and optionally simulates mouse movement to avoid auto-away status.

## 🚀 Features

- Prevents Windows from going to sleep
- Optional mouse movement to keep collaboration tools (like Teams) showing "Available"
- Lightweight and runs in the background
- Supports idle-time checks to act only when you're truly inactive

## ⚙️ How It Works

Sleppy offers two modes:

1. **Passive Mode** — Prevents sleep using Windows APIs (no visible activity).
2. **Active Mode** — Moves the mouse slightly if you're inactive, keeping your status as "Available".

It uses the native Windows `SetThreadExecutionState` API with `ES_CONTINUOUS | ES_SYSTEM_REQUIRED | ES_DISPLAY_REQUIRED` to prevent sleep and display turn-off. This call is refreshed periodically.
