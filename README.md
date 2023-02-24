# GameServerGraphic
This repository is a Server programmed in C# for a stratetic, combat based Game. 

## The Server
The Server is implented in .NET Core. The Server can communicate with a Game over TCP or UDP. Different Packets, which have different meanings, can be send and recieved over TCP or UDP. 
The Code for the Server can be found [here](GameServerGraphic/Unity/Server).
Register an Account or Login to an Account is also implemented with a SQL Database. The Server has a Graphical Interface, so that e.g. a Game can be visualized on the Server.
## The Game
The Game is implemented in Unity and can not be found in this Repository. But all the Game Logic is implemented on the Server. This means Pathfinding with the AStar Algorithm, Attacking, Troop Formation etc.
The Game is a stratetic, combat based Game. 
