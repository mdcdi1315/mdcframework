# The ExternalHashCaculators Namespace:

~~~C#
namespace ExternalHashCalculators
~~~

## The BLAKE2S Namespace:

This namespace hashes data using the BLAKE2S algorithm provided from Dustin Sparks (See at the root README file for link to it) , 

which is a relatively good enough algorithm to use for typical cryptographic operations.

To use this algorithm , use one of the `Hash` or `ComputeHash` overloads found under the `Blake2S` class.

This implementation additionally includes support for using this cryptographic algorithm as an specially implemented
[`HashAlgorithm`](http://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.hashalgorithm?view=netframework-4.8) class.

To use it , you must create a new instance of the `Hasher` class , then use the function `AsHashAlgorithm()` to get 
an `HashAlgorithm` implementation.

## The NullFXCRC Namespace:

This namespace hashes data using a base model of the CRC algorithm provided by Steve Whitley (See at the root README file for link to it).
Iterating through the namespace , you will find out that three classes exist , `CRC8` , `CRC16` and `CRC32` .
Although the first two are rarely used , some explanation for the `CRC16` class will be given , 
because `CRC16` is not one algorithm implementation , but many ones as it look like.

All the hashing methods are only overloads of the name `ComputeChecksum` 
and these are only the accessible ones.
Additionally , no object initialisation is required because the methods are all static (`Shared` in Visual Basic).

`CRC16` class has five common and different approaches of CRC16 computation:

`Crc16Agorithm.Standard` will use a CRC 16 using ~x^16~ + ~x^15~ + ~x^2~ + 1 polynomial with an initial CRC value of 0.

`Crc16Agorithm.Ccitt` will use a CRC 16 CCITT Utility using ~x^16~ + ~x^15~ + ~x^2~ + 1 polynomial with an initial CRC value of 0 , 
which it's primary usage is located in XMODEM, Bluetooth PACTOR, SD, DigRF and other communication means.

`Crc16Algorithm.CcittKermit` will use a CRC 16 CCITT Kermit using a reversed ~x^16~ + ~x^15~ + ~x^2~ + 1 polynomial with an initial CRC value of 0.

`Crc16Algorithm.Dnp` will produce a CRC 16 using reversed ~x^16~ + ~x^13~ + ~x^12~ + ~x^11~ + ~x^10~ + ~x^8~ + ~x^6~ + ~x^5~ + ~x^2~ + 1 (0xA6BC) with an initial CRC value of 0 ,
which is used in Distributed Network Protocol communications.

`Crc16Algorithm.ModBus` will produce a CRC 16 using ~x^16~ + ~x^15~ + ~x^2~ + 1 polynomial with an initial CRC value of 0xffff , 
wich is used in Modbus communications.

To use these algorithms , specify at the first parameter of the `ComputeChecksum` your desired
algorithm , including the data along to hash.

### End of the `ExternalHashCalculators` Documentation for now. Last Update Time: 25/8/2023 , 12:10 EST (UTC+02:00).