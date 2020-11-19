# JesterCap
JesterCap is a helper application that runs alongside [Spelunky 2](https://store.steampowered.com/app/418530/Spelunky_2/), a game developed by [Mossmouth](http://mossmouth.com/) and [Blitworks](https://www.blitworks.com/). There's an item in Spelunky 2 called the [True Crown](https://spelunky.fandom.com/wiki/The_True_Crown) that causes the player to teleport every 22 seconds. Bringing the True Crown to the game's true ending is an additional challenge for high-level players. JesterCap helps players manage their teleports by hooking into the Spelunky 2 process and playing an audio cue when the teleport is about to happen.

![The True Crown](/JesterCap/Resources/true_crown_icon.png "The True Crown from Spelunky 2")

## Usage
![JesterCap UI](/screencap.jpg "JesterCap UI")
* Download the latest release and run the .exe file.
* Ensure Spelunky 2 is running.
* Click the "Attach" button in the bottom of the left-hand panel. This attaches JesterCap to the Spelunky 2 process.
  * Once JesterCap is attached, you can look at the bottom of the middle panel to confirm that it is displaying your current score and updating when you pick up more gold.
* Collect the True Crown during a run. JesterCap will automatically save the time it was picked up and start the timer. 5 seconds before you teleport, you will hear a notification sound.

## Additional Features
* Automatically detects when the True Crown is collected, no need to manually sync at all
* Doesn't count time while the game is paused, helping to prevent desyncs
* Timer resets upon entering a new level
* The `notification.wav` file can be replaced to an alert sound of your choosing

## Future Improvements
* Ability to configure when the notification sound is played, or have multiple notification sounds
* Bugfixes. Testing the True Crown is hard (even using the `ABACAB00` seed).
* Automatic attachment to the Spelunky 2 process instead of requiring a button press
* UI polish. It's a bit rough around the edges currently.
* Code cleanup / refactoring
* Because Spelunky 2 is so difficult, I would love to turn this program into a trainer / assist mode for the game. However, that will require a lot of low-level assembly debugging.