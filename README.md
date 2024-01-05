# atom
json has too much bullshit in it, I want a human-writable dataformat with as few control characters as possible

Atoms are just a list of string values, and also a list of atom children

dead simple, handle the reflection yourself

  why handle it yourself? so you can format things in whatever way makes sense for your datastructs
  
  explain myself? no. I added some simple use case examples tho, hopefully this helps.
  

use tabs to create a tokenized heirachy - indent level indicates parent/child relationship

// creates a comment - anything after the slashes on a line is ignored

◘ (Alt-Num8 on windows) toggles String Literal mode - while literalized, control characters (comment slashes, newlines, etc) are ignored and everything is put into the value

that's it.

oh yeah, if you do the wrong number of indents, the parser won't shit the bed, but.... don't do it anyway.
