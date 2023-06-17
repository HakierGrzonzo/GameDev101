# GameDev101

A simple unity multiplayer game. More of a tech demo then a game.

## Controls:

Host must press `space` to start the game.


Click to move selected units.

Click units to select/unselect them.

Press `space` to unselect all units.

## How to win:

Get to the other players base, last player standing wins.

## How to connect to the host:

Start the game with the following command line flags for host:

```sh
./game --mode host
```

Then connect the clients by launching them with the following command line flags:

```sh
./game --mode client --ip <host ip>
```

The game runs on port 2137.


