# JoJo ASBR Save Tool

Tool to decrypt/encrypt JoJo ASBR PC save files

# Installing

Install the [.NET 6.0 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0/runtime) (for "console apps") before using this program.

# Usage

Use the `help` command for complete usage.

- Decryption

    `JoJoASBRSaveTool.exe dec <input_file> <output_file> [--alt]`
    
    Decrypts a save file and writes it to the output path.

    Example: `JoJoASBRSaveTool.exe dec JOJOASB.S JOJOASB.S.decrypted`
    
    Use the `--alt` option for decrypting save files other than `JOJOASB.S` (`BattleRecord`, etc.)
    
    Example: `JoJoASBRSaveTool.exe dec BattleRecord BattleRecord.decrypted --alt`

- Encryption

    `JoJoASBRSaveTool.exe enc <input_file> <output_file> [--alt]`
  
    Encrypts a save file, calculates its hash, and writes it to the output path.
  
    Example: `JoJoASBRSaveTool.exe enc JOJOASB.S.decrypted JOJOASB.S.new`
   
    Same note about `--alt` option above applies here too.
