# JesterCap
JesterCap is a helper application that runs alongside [Spelunky 2](https://store.steampowered.com/app/418530/Spelunky_2/), a game developed by [Mossmouth](http://mossmouth.com/) and [Blitworks](https://www.blitworks.com/). There's an item in Spelunky 2 called the [True Crown](https://spelunky.fandom.com/wiki/The_True_Crown) that causes the player to teleport every 22 seconds. Bringing the True Crown to the game's true ending is an additional challenge for high-level players. JesterCap helps players manage their teleports by hooking into the Spelunky 2 process and playing an audio cue when the teleport is about to happen.

![The True Crown](/JesterCap/Resources/true_crown_icon.png "The True Crown from Spelunky 2")

## Prominent Runs
* Shortly after being released, this timer was used by [TwiggleSoft](https://www.twitch.tv/twigglesoft) to set the (former) [score world record of $5,555,257](https://www.youtube.com/watch?v=MiJ2ApdaDD4).

## Usage
![JesterCap UI](/screencap.jpg "JesterCap UI")
* Download and install the latest [.NET Framework](https://dotnet.microsoft.com/download/dotnet-framework/) (4.8+).
* Download the [latest release](https://github.com/crashb/jestercap/releases/), extract to a folder on your computer, and run the .exe file.
* Ensure Spelunky 2 is running.
* Click the "Attach" button in the bottom of the left-hand panel. This attaches JesterCap to the Spelunky 2 process.
  * Once JesterCap is attached, you can look at the bottom of the middle panel to confirm that it is displaying your current score and updating when you pick up more gold.
* Collect the True Crown during a run. JesterCap will automatically save the time it was picked up and start the timer. You will start to hear notification sounds depending on the configurations specified in the included `JesterCap.exe.config` file.

## Additional Features
* Detects when the True Crown is collected, no need to manually sync at all
* Detects when the True Crown is lost (i.e. player died) and resets automatically
* Doesn't count time while the game is paused or during Ankh respawn animation, helping to prevent desyncs
* Timer resets upon entering a new level
* The `Resources/notification.wav` file can be replaced to an alert sound of your choosing
* The `JesterCap.exe.config` file can be edited to change when & how many times the alert sound fires

## Known Issues
* Attaching to the Spelunky 2 process while on a True Crown run will cause the countdown timer to display incorrectly. This happens because there's no way for JesterCap to detect when the True Crown was picked up if JesterCap wasn't running at the time. To work around this problem, make sure JesterCap is running when the True Crown is picked up.
  * This issue will resolve itself upon moving to the next level.
* Local multiplayer / online is unsupported at this time.

## Future Improvements
* Bugfixes. Testing the True Crown is hard (even using seeded runs).
* Automatic attachment to the Spelunky 2 process instead of requiring a button press
* UI polish. It's a bit rough around the edges currently.
* Code cleanup / refactoring
* Local multiplayer / online support