# WFCLevelGenerator

Unity plugin that generates game levels.

The generator is based on the wave function collapse algorithm.

## Using the generator  

The generator consists of two parts: the input model and the constraint solver. The input model uses a json document that declares the allowed adjacencies for different tiles. This requires creating tiles and defining their adjacencies with neighboring tiles.

### Generator parameters

- jsonFile: input data, *.json file. It enumerates tiles, tile symmetry, tile weights, neighborhood rules.
- LevelMap: width, height (int) size of the level. OutputTilemap - Tilemap, on which the level will be drawn. InputTilemap - Tilemap, where the tiles are preset.
- Subset (string) name of the set of tiles from the *.json configuration.
- Seed (int) all internal random values are derived from this initial number, providing 0 results in a random number.
- limit (int) how many iterations to execute, providing 0 until completion or contradiction.
- Periodic (bool) determines whether the output solutions are mosaic. This is useful for creating things like mosaic textures, but also has an unexpected effect on the result. When working with WFC, it is often advisable to turn periodic output on and off, checking to see if any parameter affects the results in a favorable way.
- Iterations (int) is how many iterations to run, providing 0 will run until completion or a contradiction.
- InstantTilemapCollider (bool) sets CompositeCollider2D on outputTilemap.

### How to work with the generator

1. Create a set of tiles.
2. Specify a set of tiles with symmetry and weights, define and specify neighborhood rules in *.json.
3. Add the LevelGenerator component to the Tilemap grid.
4. Specify the necessary parameters for the generator.
5. Press the "generate" button or run the scene.

## Contact

[GitHub][git-repo-url]

## License

MIT

[git-repo-url]: <https://github.com/Alexandr-star/WFCLevelGenerator>