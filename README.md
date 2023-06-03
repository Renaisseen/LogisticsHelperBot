# LogisticHelperBot
Telegram Bot + Google Direction API that searches for fastest route from point A to point B with different possible cities between them
# Language
Written in C# using Google namespace for Direction API usage. Replace key for Google API and Telegram token for personal use.
# HowTo
Copy project to your local directory, replace tokens in Program.cs, run. Go to Telegram, write bot commands. REPLY to bot messages to move along algorithm.<br>Currently only one "between" city among entered will take place in optimal route.
# Example
**You**: /analyze<br>
**Bot**: Enter the starting point (Point A).<br>
**You**: reply: Kyiv<br>
**Bot**: Enter the destination (Point B).<br>
**You**: reply: Lviv <br>
**Bot**: Enter possible cities for staying the night (separated by commas).<br>
**You**: reply: Vinnytsia,Cherkasy,Uman',Korosten'<br>
**Bot**: Fastest route is:<br>
Route: Kyiv - Korosten' Dur: 02h:12m:19s<br>
Route: Korosten' - Lviv Dur: 05h:27m:02s<br>
