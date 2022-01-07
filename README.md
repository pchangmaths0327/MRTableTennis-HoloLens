# MRTableTennis
ETH MR Lab course project AS2021: Augmented Table Tennis Game  
This repository contains the Unity project for HoloLens 2 associated with the project

Authors: Tianxu An, Pascal Chang, Matthias Koenig, Severin Laasch


# Getting started

1. Clone this repository
2. Add the project to Unity Hub
3. Open the project (Unity version: 2020.3.19f1)
4. In `Assets/Scenes/` open `MainScene.unity`

# Scene Structure

There are 4 "sub-scenes" in `MainScene.unity`:

1. **IntroMenu**: shows the Welcome Message and ask to scan phone to start. For now, since the connection to the phone is not integrated yet, the messages appear 2 seconds after the game starts and stays for roughly 10 seconds before fading out. See `IntroMenuScript.cs` attached to the `IntroMenu` GameObject for more details.
2. **PlayerMenu**: shows two buttons for selecting 1 or 2 players (number of players wearing HoloLens). In theory, selecting "2 Players" should lead to an interface where the HoloLens tries to connect with the other one. For now, clicking on either one leads to the next sub-scene directly.
3. **TableDetectionMenu**: tries to detect platforms in the scene and fit the table plane to it. When the table is detected, the message changes from "Detecting Table..." to "Table Detected!". The user can then manipulate it to fit it more accurately. This part uses the `Windows Scene Understanding Observer` (see `Mixed Reality Toolkit > Spatial Awareness`). A small menu follows the user allowing to refresh detection, hide/show the table and apply the changes to go to the next scene.
4. **PlayingScene**: this section is activated once the table is detected and the user selects "Done". The table becomes green and cannot be manipulated anymore. A message indicates the game can start.
