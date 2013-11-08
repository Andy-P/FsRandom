﻿[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module FsRandom.String

let ascii = [|'!' .. '~'|]
let digit = [|'0' .. '9'|]
let upper = [|'A' .. 'Z'|]
let lower = [|'a' .. 'z'|]
let alphabet = Array.concat [upper; lower]
let alphanumeric = Array.concat [digit; alphabet]

let inline makeString (array:char []) = System.String (array)
let inline randomStringByCharArray array length =
   Random.transformBy makeString (Array.sampleWithReplacement length array)

let randomByString (s:string) length = randomStringByCharArray (s.ToCharArray ()) length
let randomAscii length = randomStringByCharArray ascii length
let randomNumeric length = randomStringByCharArray digit length
let randomAlphabet length = randomStringByCharArray alphabet length
let randomAlphanumeric length = randomStringByCharArray alphanumeric length
let randomConcat separator randomStringGenerators = Random.mergeWith (String.concat separator) randomStringGenerators
