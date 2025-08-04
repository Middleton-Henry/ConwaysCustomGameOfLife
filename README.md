# Conway's Custom Game Of Life
This is a customizable version of Conway's Game of Life by Middleton Henry based on the original version by John Conway. This simulation/game is meant to create a cellular automaton system that can grow and morph into unique and unpredictable patterns based on a simple set of rules listed below.

This project is available for download on [itch.io](https://middleton-henry.itch.io/conways-custom-game-of-life)

Currently the online build does not support copying and pasting save states

# Key Binds: 

Left Mouse Click - Add living cell

Right Mouse Click - Add dead cell

Right Arrow - Forward 1 generation

Left Arrow - Reset to 1st generation

'A' - Toggle Automatic Updates

'R' - clear grid

'S' - save state

'L' - load state

'D' - Distribute random values


# Rules

1) Any live cell with fewer than two live neighbours dies, as if by underpopulation.
2) Any live cell with two or three live neighbours lives on to the next generation.
3) Any live cell with more than three live neighbours dies, as if by overpopulation.
4) Any dead cell with exactly three live neighbours becomes a live cell, as if by reproduction.

For the purpose of this customizable version, these rules can be rewritten as such:

1) A cell with 2 live neighbours remains the same in the next generation
2) A cell with 3 live neighbours will be alive in the next generation
3) A cell with any other number of live neighbours will be dead next generation (0-1, 4-8)

This version gives us 3 possible state evaluations – alive, same, and dead – which can be checked depending on the number of living neighbours. Also, the range of checked neighbours can be extended or shortened from the original 8. WebGL/Itch.io seems to have some limitations in term of how many images can be download at once, so you might need to input each separately.

# Additional Options

Distribute values - Distributes random assortment of values based on percentage 

Render image - saves images of game state based on input generation range below 

(the current version only creates png's, so if you want a gif version then I recommend using [ezgif.com](https://ezgif.com/maker) which allows up to 2000 files)

Custom Palette Swap - Insert custom RGB values or hit button for random colors

Save/Load States - Use input field to save and load custom settings and cell placement
(Copy and Paste does not work with Unity TMP Input Fields in WebGL, so use download for this functionality)
