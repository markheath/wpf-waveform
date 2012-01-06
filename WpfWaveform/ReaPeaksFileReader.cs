using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace WpfWaveform
{
    struct Peak16
    {
        public short Min;
        public short Max;
    }

    class PeakValues
    {
        public PeakValues(int channelCount)
        {
            this.Channels = new Peak16[channelCount];
        }
        public Peak16[] Channels { get; private set; }
    }

    class MipMap
    {
        /// <summary>
        /// Number of samples per peak
        /// </summary>
        public int DivisionFactor { get; set; }

        public PeakValues[] Peaks { get; set; }
    }

    class ReaPeaksFileReader
    {
        private string magicHeader;
        private int channels;
        private int sampleRate;

        public MipMap[] MipMaps { get; private set; }

        // http://www.reaper.fm/sdk/reapeaks.txt
        public ReaPeaksFileReader(string filename)
        {
            var mipMaps = new List<MipMap>();
            using (BinaryReader reader = new BinaryReader(File.OpenRead(filename)))
            {
                this.magicHeader = Encoding.ASCII.GetString(reader.ReadBytes(4));
                if (magicHeader != "RPKM" && magicHeader != "RPKN")
                {
                    throw new InvalidOperationException("Not a valid ReaPeaks file");
                }
                this.channels = reader.ReadByte();
                int mipMapCount = reader.ReadByte();
                this.sampleRate = reader.ReadInt32();
                int sourceTimestamp = reader.ReadInt32();
                int sourceFilesize = reader.ReadInt32();
                for (int mipMap = 0; mipMap < mipMapCount; mipMap++)
                {
                    mipMaps.Add(ReadMipMapHeader(reader));
                }
                for (int mipMap = 0; mipMap < mipMapCount; mipMap++)
                {
                    ReadMipMapData(reader, mipMaps[mipMap]);
                }
            }
            this.MipMaps = mipMaps.ToArray();
        }

        private MipMap ReadMipMapHeader(BinaryReader reader)
        {
            var mipMap = new MipMap();
            mipMap.DivisionFactor = reader.ReadInt32(); // number of samples per peak
            int numberOfPeakSamples = reader.ReadInt32();
            mipMap.Peaks = new PeakValues[numberOfPeakSamples];
            return mipMap;
        }

        private void ReadMipMapData(BinaryReader reader, MipMap mipMap)
        {
            for (int n = 0; n < mipMap.Peaks.Length; n++)
            {
                mipMap.Peaks[n] = new PeakValues(channels);
                for (int ch = 0; ch < channels; ch++)
                {
                    mipMap.Peaks[n].Channels[ch].Max = reader.ReadInt16();
                    if (magicHeader == "RPKN") // 1.1
                    {
                        mipMap.Peaks[n].Channels[ch].Min = reader.ReadInt16();
                    }
                    else // 1.0
                    {
                        mipMap.Peaks[n].Channels[ch].Min = (short)(0 - mipMap.Peaks[n].Channels[ch].Max);
                    }
                }
            }
        }
    }
}
