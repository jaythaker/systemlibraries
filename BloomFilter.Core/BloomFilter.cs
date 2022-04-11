using System.Collections;
using System.Text;

namespace SystemCore.DataStructure;

public class BloomFilter
{
    private uint _expectedElements;
    private double _falsePositiveProbability;
    private int _hashCount;
    private BitArray _bitArray;
    private readonly Murmur.Murmur32 _murmur = Murmur.MurmurHash.Create32();

    public BloomFilter(double falsePositiveProbability, uint exceptedElements)
    {
        if (exceptedElements == 0) throw new ArgumentException(nameof(exceptedElements));
        _expectedElements = exceptedElements;
        _falsePositiveProbability = falsePositiveProbability;
        int size = (int)(-1 * (exceptedElements * Math.Log2(falsePositiveProbability) / (Math.Log2(2) * Math.Log2(2))));
        _hashCount = (int)((size / exceptedElements) * Math.Log2(2));
        _bitArray = new BitArray(size);
    }

    public void Add(string data)
    {
        if (string.IsNullOrWhiteSpace(data))  throw new ArgumentNullException(nameof(data));
        
        var bitIndexes = GetHashIndexes(data);
        foreach (var index in bitIndexes)
            _bitArray.Set(index, true);
    }

    public bool IsPresent(string data)
    {
        if (string.IsNullOrWhiteSpace(data))  throw new ArgumentNullException(nameof(data));
        
        var bitIndexes = GetHashIndexes(data);
        var result = false;
        foreach (var index in bitIndexes)
        {
            result = _bitArray.Get(index);
            if (!result) break;
        }
        return result == true;
    }

    private int[] GetHashIndexes(string data)
    {
        if (string.IsNullOrWhiteSpace(data))  throw new ArgumentNullException(nameof(data));
        
        var result = new int[_hashCount];
        var firstHash = Math.Abs(data.GetHashCode());

        result[0] = firstHash % _bitArray.Count;

        var secondHash = Math.Abs(BitConverter.ToInt32(_murmur.ComputeHash(UTF8Encoding.UTF8.GetBytes(data))));

        result[1] = secondHash % _bitArray.Count;

        var hashRand = new Random(secondHash);
        for (int i = 3; i < _hashCount + 1; i++)
        {
            int nextHash = Math.Abs(hashRand.Next());
            result[i - 1] = nextHash % _bitArray.Count;
        }

        return result;
    }
}
