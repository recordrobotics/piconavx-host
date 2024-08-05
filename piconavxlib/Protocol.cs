namespace piconavx
{
    public static class Protocol
    {
        const string HOST_INTRODUCTION = "PICONAVX-HOST";

        const char DATA_FIELD_SEPARATOR = '|';
        const char DATA_FEED_TIMESTAMP_SEPARATOR = ':';
        const char DATA_FEED_ENTRY_SEPARATOR= ';';

        const string TYPE_ID = "ID:";
        const string TYPE_RAW = "RAW:";
        const string TYPE_AHRS = "AHRS:";
        const string TYPE_AHRSPOS = "AHRSPOS:";
        const string TYPE_YPR = "YPR:";
        const string TYPE_HEALTH = "HEALTH:";
        const string TYPE_BOARDSTATE = "BSTATE:";
        const string TYPE_BOARDID = "BID:";
        const string TYPE_FEED = "FEED:";

        const string COMMAND_SET_DATA_TYPE = "SETDATA:";
        const string COMMAND_SET_DATA_TYPE_RAW = "RAW";
        const string COMMAND_SET_DATA_TYPE_AHRS = "AHRS";
        const string COMMAND_SET_DATA_TYPE_AHRSPOS = "AHRSPOS";
        const string COMMAND_SET_DATA_TYPE_YPR = "YPR";
        const string COMMAND_SET_DATA_TYPE_FEED = "FEED";

        const string COMMAND_REQUEST_HEALTH = "GETHEALTH:";
        const string COMMAND_REQUEST_BOARDSTATE = "GETBSTATE:";
        const string COMMAND_REQUEST_BOARDID = "GETBID:";
        const string COMMAND_ZERO_YAW = "ZEROYAW:";
        const string COMMAND_ZERO_DISPLACEMENT = "ZERODISP:";

        const string COMMAND_SET_FEED_OVERFLOW = "SETFEEDOVF:";
        const string COMMAND_SET_FEED_OVERFLOW_DELETE_OLDEST = "DELETE";
        const string COMMAND_SET_FEED_OVERFLOW_REDUCE_OLDEST_FREQUENCY = "LOWFREQ";
        const string COMMAND_SET_FEED_OVERFLOW_SKIP = "SKIP";

        public static async Task<bool> IdentifyClient(Client client, CancellationToken cancellationToken)
        {
            if (client.IsVirtual)
                throw new InvalidOperationException("Can't identify a virtual client. Instead, set the id directly.");

            await client.Writer.WriteLineAsync(HOST_INTRODUCTION); // Introduce ourselves
            await client.Writer.FlushAsync(cancellationToken);
            string? response = await client.Reader.ReadLineAsync(cancellationToken);

            if (response == null || ParseDataType(response) != DataType.Id)
                return false;

            client.Id = ParseId(response);

            return true;
        }

        public static DataType ParseDataType(string? line)
        {
            if (line == null)
                return DataType.Unknown;

            if (line.StartsWith(TYPE_ID))
            {
                return DataType.Id;
            }
            else if (line.StartsWith(TYPE_RAW))
            {
                return DataType.RawUpdate;
            }
            else if (line.StartsWith(TYPE_AHRS))
            {
                return DataType.AHRSUpdate;
            }
            else if (line.StartsWith(TYPE_AHRSPOS))
            {
                return DataType.AHRSPosUpdate;
            }
            else if (line.StartsWith(TYPE_YPR))
            {
                return DataType.YPRUpdate;
            }
            else if (line.StartsWith(TYPE_HEALTH))
            {
                return DataType.HealthUpdate;
            }
            else if (line.StartsWith(TYPE_BOARDSTATE))
            {
                return DataType.BoardStateUpdate;
            }
            else if (line.StartsWith(TYPE_BOARDID))
            {
                return DataType.BoardIdUpdate;
            }
            else if (line.StartsWith(TYPE_FEED))
            {
                return DataType.FeedUpdate;
            }
            else
            {
                return DataType.Unknown;
            }
        }

        public static string ParseId(string line)
        {
            return line.Substring(TYPE_ID.Length);
        }

        public static HealthUpdate ParseHealthUpdate(string line)
        {
            line = line.Substring(TYPE_HEALTH.Length);
            string[] parts = line.Split(DATA_FIELD_SEPARATOR);
            return new HealthUpdate()
            {
                MemoryUsed = int.Parse(parts[0]),
                MemoryTotal = int.Parse(parts[1]),
                CoreTemp = double.Parse(parts[2])
            };
        }

        public static YPRUpdate ParseYPRUpdate(string line)
        {
            line = line.Substring(TYPE_YPR.Length);
            string[] parts = line.Split(DATA_FIELD_SEPARATOR);
            return new YPRUpdate()
            {
                Yaw = double.Parse(parts[0]),
                Pitch = double.Parse(parts[1]),
                Roll = double.Parse(parts[2])
            };
        }

        public static RawUpdate ParseRawUpdate(string line)
        {
            line = line.Substring(TYPE_RAW.Length);
            string[] parts = line.Split(DATA_FIELD_SEPARATOR);
            return new RawUpdate()
            {
                GyroX = short.Parse(parts[0]),
                GyroY = short.Parse(parts[1]),
                GyroZ = short.Parse(parts[2]),
                AccelX = short.Parse(parts[3]),
                AccelY = short.Parse(parts[4]),
                AccelZ = short.Parse(parts[5]),
                MagX = short.Parse(parts[6]),
                MagY = short.Parse(parts[7]),
                MagZ = short.Parse(parts[8]),
                TempC = double.Parse(parts[9]),
            };
        }

        public static AHRSPosUpdate ParseAHRSPosUpdate(string line)
        {
            line = line.Substring(TYPE_AHRSPOS.Length);
            string[] parts = line.Split(DATA_FIELD_SEPARATOR);
            return new AHRSPosUpdate()
            {
                Yaw = double.Parse(parts[0]),
                Pitch = double.Parse(parts[1]),
                Roll = double.Parse(parts[2]),
                CompassHeading = double.Parse(parts[3]),
                Altitude = double.Parse(parts[4]),
                FusedHeading = double.Parse(parts[5]),
                LinearAccelX = double.Parse(parts[6]),
                LinearAccelY = double.Parse(parts[7]),
                LinearAccelZ = double.Parse(parts[8]),
                MpuTemp = double.Parse(parts[9]),
                QuatW = double.Parse(parts[10]),
                QuatX = double.Parse(parts[11]),
                QuatY = double.Parse(parts[12]),
                QuatZ = double.Parse(parts[13]),
                BarometricPressure = double.Parse(parts[14]),
                BaroTemp = double.Parse(parts[15]),
                OpStatus = (NavXOPStatus)byte.Parse(parts[16]),
                SensorStatus = (NavXSensorStatus)byte.Parse(parts[17]),
                CalStatus = (NavXCalStatus)byte.Parse(parts[18]),
                SelfTestStatus = (NavXSelftestStatus)byte.Parse(parts[19]),
                VelX = double.Parse(parts[20]),
                VelY = double.Parse(parts[21]),
                VelZ = double.Parse(parts[22]),
                DispX = double.Parse(parts[23]),
                DispY = double.Parse(parts[24]),
                DispZ = double.Parse(parts[25])
            };
        }

        public static AHRSUpdate ParseAHRSUpdate(string line)
        {
            line = line.Substring(TYPE_AHRS.Length);
            string[] parts = line.Split(DATA_FIELD_SEPARATOR);
            return new AHRSUpdate()
            {
                Yaw = double.Parse(parts[0]),
                Pitch = double.Parse(parts[1]),
                Roll = double.Parse(parts[2]),
                CompassHeading = double.Parse(parts[3]),
                Altitude = double.Parse(parts[4]),
                FusedHeading = double.Parse(parts[5]),
                LinearAccelX = double.Parse(parts[6]),
                LinearAccelY = double.Parse(parts[7]),
                LinearAccelZ = double.Parse(parts[8]),
                MpuTemp = double.Parse(parts[9]),
                QuatW = double.Parse(parts[10]),
                QuatX = double.Parse(parts[11]),
                QuatY = double.Parse(parts[12]),
                QuatZ = double.Parse(parts[13]),
                BarometricPressure = double.Parse(parts[14]),
                BaroTemp = double.Parse(parts[15]),
                OpStatus = (NavXOPStatus)byte.Parse(parts[16]),
                SensorStatus = (NavXSensorStatus)byte.Parse(parts[17]),
                CalStatus = (NavXCalStatus)byte.Parse(parts[18]),
                SelfTestStatus = (NavXSelftestStatus)byte.Parse(parts[19]),
                CalMagX = double.Parse(parts[20]),
                CalMagY = double.Parse(parts[21]),
                CalMagZ = double.Parse(parts[22]),
                MagFieldNormRatio = double.Parse(parts[23]),
                MagFieldNormScalar = double.Parse(parts[24]),
                RawMagX = double.Parse(parts[25]),
                RawMagY = double.Parse(parts[26]),
                RawMagZ = double.Parse(parts[27])
            };
        }

        public static BoardStateUpdate ParseBoardStateUpdate(string line)
        {
            line = line.Substring(TYPE_BOARDSTATE.Length);
            string[] parts = line.Split(DATA_FIELD_SEPARATOR);
            return new BoardStateUpdate()
            {
                OpStatus = (NavXOPStatus)byte.Parse(parts[0]),
                SensorStatus = (NavXSensorStatus)ushort.Parse(parts[1]),
                CalStatus = (NavXCalStatus)byte.Parse(parts[2]),
                SelftestStatus = (NavXSelftestStatus)byte.Parse(parts[3]),
                CapabilityFlags = (NavXCapabilityFlags)ushort.Parse(parts[4]),
                UpdateRateHz = byte.Parse(parts[5]),
                AccelFsrG = byte.Parse(parts[6]),
                GyroFsrDps = ushort.Parse(parts[7])
            };
        }

        public static BoardIdUpdate ParseBoardIdUpdate(string line)
        {
            line = line.Substring(TYPE_BOARDID.Length);
            string[] parts = line.Split(DATA_FIELD_SEPARATOR);
            return new BoardIdUpdate()
            {
                Type = byte.Parse(parts[0]),
                HwRev = byte.Parse(parts[1]),
                FwVerMajor = byte.Parse(parts[2]),
                FwVerMinor = byte.Parse(parts[3]),
                FwRevision = byte.Parse(parts[4])
            };
        }

        public static async Task<FeedChunk[]> ParseFeedUpdate(string line, Stream stream, CancellationToken token)
        {
            line = line.Substring(TYPE_FEED.Length);
            string[] parts = line.Split(DATA_FIELD_SEPARATOR);
            int dataLength = int.Parse(parts[0]);
            int chunkCount = int.Parse(parts[1]);
            const int chunkSize = 92;

            byte[] chunkData = new byte[chunkSize];
            FeedChunk[] chunks = new FeedChunk[chunkCount];

            uint decodeTimestamp()
            {
                Span<byte> view = new Span<byte>(chunkData, 0, 4);
                if (BitConverter.IsLittleEndian)
                    view.Reverse();
                return BitConverter.ToUInt32(view);
            }

            float getFloat(int start)
            {
                Span<byte> view = new Span<byte>(chunkData, start, 4);
                if (BitConverter.IsLittleEndian)
                    view.Reverse();
                return BitConverter.ToSingle(view);
            }

            for (int i = 0; i < chunkCount; i++)
            {
                await stream.ReadExactlyAsync(chunkData, 0, chunkSize, token);
                uint timestamp = decodeTimestamp();
                AHRSPosUpdate update = new AHRSPosUpdate()
                {
                    Yaw = getFloat(4),
                    Pitch = getFloat(8),
                    Roll = getFloat(12),
                    CompassHeading = getFloat(16),
                    Altitude = getFloat(20),
                    FusedHeading = getFloat(24),
                    LinearAccelX = getFloat(28),
                    LinearAccelY = getFloat(32),
                    LinearAccelZ = getFloat(36),
                    MpuTemp = getFloat(40),
                    QuatW = getFloat(44),
                    QuatX = getFloat(48),
                    QuatY = getFloat(52),
                    QuatZ = getFloat(56),
                    BarometricPressure = getFloat(60),
                    BaroTemp = getFloat(64),
                    VelX = getFloat(68),
                    VelY = getFloat(72),
                    VelZ = getFloat(76),
                    DispX = getFloat(80),
                    DispY = getFloat(84),
                    DispZ = getFloat(88)
                };
                chunks[i] = new FeedChunk(timestamp, update);
            }

            return chunks;
        }

        public static string SerializeSetDataTypeCommand(HostSetDataType dataType)
        {
            string value = COMMAND_SET_DATA_TYPE_AHRSPOS;
            switch (dataType)
            {
                case HostSetDataType.Raw:
                    value = COMMAND_SET_DATA_TYPE_RAW;
                    break;
                case HostSetDataType.AHRS:
                    value = COMMAND_SET_DATA_TYPE_AHRS;
                    break;
                case HostSetDataType.AHRSPos:
                    value = COMMAND_SET_DATA_TYPE_AHRSPOS;
                    break;
                case HostSetDataType.YPR:
                    value = COMMAND_SET_DATA_TYPE_YPR;
                    break;
                case HostSetDataType.Feed:
                    value = COMMAND_SET_DATA_TYPE_FEED;
                    break;
            }
            return string.Format("{0}{1}", COMMAND_SET_DATA_TYPE, value);
        }

        public static string SerializeSetFeedOverflowCommand(HostSetFeedOverflowType feedOverflow)
        {
            string value = COMMAND_SET_FEED_OVERFLOW_DELETE_OLDEST;
            switch (feedOverflow)
            {
                case HostSetFeedOverflowType.DeleteOldest:
                    value = COMMAND_SET_FEED_OVERFLOW_DELETE_OLDEST;
                    break;
                case HostSetFeedOverflowType.ReduceOldestFrequency:
                    value = COMMAND_SET_FEED_OVERFLOW_REDUCE_OLDEST_FREQUENCY;
                    break;
                case HostSetFeedOverflowType.Skip:
                    value = COMMAND_SET_FEED_OVERFLOW_SKIP;
                    break;
            }
            return string.Format("{0}{1}", COMMAND_SET_FEED_OVERFLOW, value);
        }

        public static string SerializeRequestHealthCommand()
        {
            return COMMAND_REQUEST_HEALTH;
        }

        public static string SerializeRequestBoardStateCommand()
        {
            return COMMAND_REQUEST_BOARDSTATE;
        }

        public static string SerializeRequestBoardIdCommand()
        {
            return COMMAND_REQUEST_BOARDID;
        }

        public static string SerializeZeroYawCommand()
        {
            return COMMAND_ZERO_YAW;
        }

        public static string SerializeZeroDisplacementCommand()
        {
            return COMMAND_ZERO_DISPLACEMENT;
        }
    }

    public enum DataType
    {
        Id,
        RawUpdate,
        AHRSUpdate,
        AHRSPosUpdate,
        YPRUpdate,
        HealthUpdate,
        BoardStateUpdate,
        BoardIdUpdate,
        FeedUpdate,
        Unknown
    }

    public enum NavXCalStatus : byte
    {
        InProgress = 0x00,
        Accumulate = 0x01,
        Complete = 0x02
    }

    [Flags]
    public enum NavXSelftestStatus : byte
    {
        GyroPassed = 0x01,
        AccelPassed = 0x02,
        MagPassed = 0x04,
        BaroPassed = 0x08,
        Complete = 0x80
    }

    public enum NavXOPStatus : byte
    {
        Initializing = 0x00,
        SelftestInProgress = 0x01,
        Error = 0x02,
        IMUAutocalInProgress = 0x03,
        Normal = 0x04
    }

    [Flags]
    public enum NavXSensorStatus : ushort
    {
        Moving = 0x01,
        YawStable = 0x02,
        MagDisturbance = 0x04,
        AltitudeValid = 0x08,
        SealevelPressSet = 0x10,
        FusedHeadingValid = 0x20
    }

    [Flags]
    public enum NavXCapabilityFlags : ushort
    {
        Omnimount = 0x0004,
        OmnimountDefault = (0 << 3),
        OmnimountYawXUp = (1 << 3),
        OmnimountYawXDown = (2 << 3),
        OmnimountYawYUp = (3 << 3),
        OmnimountYawYDown = (4 << 3),
        OmnimountYawZUp = (5 << 3),
        OmnimountYawZDown = (6 << 3),
        VelAndDisp = 0x0038,
        YawReset = 0x0080,
        AHRSPosTs = 0x0100,
        FWRevision = 0x0200,
        HiResTimestamp = 0x0400,
        AHRSPosTsRaw = 0x0800
    }

    public enum HostCommandType
    {
        SetDataType,
        RequestHealth,
        RequestBoardState,
        RequestBoardId,
        ZeroYaw,
        ZeroDisplacement,
        SetFeedOverflow
    }

    public enum HostSetDataType
    {
        Raw,
        AHRS,
        AHRSPos,
        YPR,
        Feed
    }

    public enum HostSetFeedOverflowType
    {
        DeleteOldest,
        ReduceOldestFrequency,
        Skip
    }
}
