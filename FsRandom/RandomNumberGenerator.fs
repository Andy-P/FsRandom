﻿module FsRandom.RandomNumberGenerator

open System
open FsRandom.StateMonad

type Prng<'s> = 's -> uint64 * 's
type PrngState<'s> = Prng<'s> * 's

let random = state

type RandomBuilder<'s> (prng:Prng<'s>) =
   inherit StateBuilder ()
   member this.Run (m) = fun (seed:'s) -> m (prng, seed) |> (fun (random, (_, state) : PrngState<'s>) -> random, state)
let createRandomBuilder prng = RandomBuilder (prng)

let buffer = Array.zeroCreate sizeof<uint64>
let systemrandomPrng (random : Random) = 
   random.NextBytes (buffer)
   BitConverter.ToUInt64 (buffer, 0), random
let systemrandom = createRandomBuilder systemrandomPrng
   
let inline xor128 (x:uint32, y:uint32, z:uint32, w:uint32) =
   let t = x ^^^ (x <<< 11)
   let (_, _, _, w') as s = y, z, w, (w ^^^ (w >>> 19)) ^^^ (t ^^^ (t >>> 8))
   w', s
let xorshiftPrng s =
   let lower, s = xor128 s
   let upper, s = xor128 s
   to64bit lower upper, s
let xorshift = createRandomBuilder xorshiftPrng

let getRandom (generator : State<PrngState<'s>, 'a >) =
   getState |>> (fun s0 -> let r, s' = generator s0 in setState s' &>> returnState r)
let getRandomBy f (generator : State<PrngState<'s>, 'a >) =
   getState |>> (fun s0 -> let r, s' = generator s0 in setState s' &>> returnState (f r))

[<Literal>]
let ``1 / 2^52`` = 2.22044604925031308084726333618e-16
[<Literal>]
let ``1 / 2^53`` = 1.11022302462515654042363166809e-16
[<Literal>]
let ``1 / (2^53 - 1)`` = 1.1102230246251566636831481e-16
let ``(0, 1)`` ((f, s0) : PrngState<'s>) = let r, s' = f s0 in (float (r >>> 12) + 0.5) * ``1 / 2^52``, (f, s')
let ``[0, 1)`` ((f, s0) : PrngState<'s>) = let r, s' = f s0 in float (r >>> 11) * ``1 / 2^53``, (f, s')
let ``(0, 1]`` ((f, s0) : PrngState<'s>) = let r, s' = f s0 in (float (r >>> 12) + 1.0) * ``1 / 2^52``, (f, s')
let ``[0, 1]`` ((f, s0) : PrngState<'s>) = let r, s' = f s0 in float (r >>> 11) * ``1 / (2^53 - 1)``, (f, s')