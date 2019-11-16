using Godot;

public class VoxelBuffer : Reference
{
    public enum ChannelId{
        CHANNEL_TYPE = 0,
		CHANNEL_ISOLEVEL,
		CHANNEL_DATA2,
		CHANNEL_DATA3,
		CHANNEL_DATA4,
		CHANNEL_DATA5,
		CHANNEL_DATA6,
		CHANNEL_DATA7,
		// Arbitrary value, 8 should be enough. Tweak for your needs.
		MAX_CHANNELS
    }

    // TODO use C++17 inline to initialize right here...
	static char CHANNEL_ID_HINT_STRING;

	enum Compression {
		COMPRESSION_NONE = 0,
		COMPRESSION_UNIFORM,
		//COMPRESSION_RLE,
		COMPRESSION_COUNT
	};

	// TODO Quantification options
	//	enum ChannelFormat {
	//		FORMAT_I8_Q256U, // 0..255 integer
	//		FORMAT_F8_Q1S, // -1..1 float stored in 8 bits
	//		FORMAT_F16_Q128S // -128..128 float stored in 16 bits
	//	};

	// Converts -1..1 float into 0..255 integer
	public static byte IsoToByte(float iso) {
		byte v = (byte)(128F * iso + 128F);
		if (v > 255)
			return 255;
		else if (v < 0)
			return 0;
		return v;
	}

	// Converts 0..255 integer into -1..1 float
	static float ByteToIso(int b) {
		return (float)(b - 128) / 128F;
	}

	public VoxelBuffer(){}

	public void create(int sx, int sy, int sz){

    }

	/*
	public void create(Vector3i size){}

	public void clear(){}

	public void ClearChannel(int channel_index, int clear_value = 0){}

	public void ClearChannelF(int channel_index, float clear_value = 0) {
         ClearChannel(channel_index, IsoToByte(clear_value)); 
    }

    //public Vector3i GetSize() { return this.(); }

	public void SetDefaultValues(int[] values){}

	public int GetVoxel(int x, int y, int z, int channel_index = 0){
        return 0;
    }
	public void SetVoxel(int value, int x, int y, int z, int channel_index = 0){}

	public void TrySetVoxel(int x, int y, int z, int value, int channel_index = 0){}

	public void SetVoxelF(float value, int x, int y, int z, int channel_index = 0) { 
        SetVoxel(IsoToByte(value), x, y, z, channel_index); 
    }
	public float GetVoxelF(int x, int y, int z, int channel_index = 0) { 
        return ByteToIso(GetVoxel(x, y, z, channel_index)); 
    }

	public int GetVoxel(Vector3i pos, int channel_index = 0) { 
        return GetVoxel(pos.x, pos.y, pos.z, channel_index); 
    }

	public void SetVoxel(int value, Vector3i pos, int channel_index = 0) { 
        SetVoxel(value, pos.x, pos.y, pos.z, channel_index); 
    }

	public void Fill(int defval, int channel_index = 0){}
    public void FillF(float value, int channel = 0) { 
        Fill(IsoToByte(value), channel); 
    }
	public void FillArea(int defval, Vector3i min, Vector3i max, int channel_index = 0){}

	public bool IsUniform(int channel_index){
        return false;
    }

	public void CompressUniformChannels(){}
	public void DecompressChannel(int channel_index){}
	public Compression GetChannelCompression (int channel_index) {
        return new VoxelBuffer.Compression();
    }

	public void GrabChannelData(int in_buffer, int channel_index, Compression compression){}

	public void CopyFrom(VoxelBuffer other, int channel_index = 0){}
	public void CopyFrom(VoxelBuffer other, Vector3i src_min, Vector3i src_max, Vector3i dst_min, int channel_index = 0){}

	public VoxelBuffer Duplicate(){
        return null;
    }

	public bool ValidatePos(int x, int y, int z) {
		return x < (unsigned)_size.x && y < (unsigned)_size.y && z < (unsigned)_size.z;
	}

	public int index(unsigned int x, unsigned int y, unsigned int z) const {
		return y + _size.y * (x + _size.x * z);
	}

	//	_FORCE_INLINE_ unsigned int row_index(unsigned int x, unsigned int y, unsigned int z) const {
	//		return _size.y * (x + _size.x * z);
	//	}

	_FORCE_INLINE_ unsigned int get_volume() const {
		return _size.x * _size.y * _size.z;
	}

	uint8_t *get_channel_raw(unsigned int channel_index) const;

	void downscale_to(VoxelBuffer &dst, Vector3i src_min, Vector3i src_max, Vector3i dst_min) const;
	Ref<VoxelTool> get_voxel_tool() const;

	bool equals(const VoxelBuffer *p_other) const;

private:
	void create_channel_noinit(int i, Vector3i size);
	void create_channel(int i, Vector3i size, uint8_t defval);
	void delete_channel(int i);

protected:
	static void _bind_methods();

	_FORCE_INLINE_ int get_size_x() const { return _size.x; }
	_FORCE_INLINE_ int get_size_y() const { return _size.y; }
	_FORCE_INLINE_ int get_size_z() const { return _size.z; }

	// Bindings
	_FORCE_INLINE_ Vector3 _b_get_size() const { return _size.to_vec3(); }
	void _b_create(int x, int y, int z) { create(x, y, z); }
	_FORCE_INLINE_ int _b_get_voxel(int x, int y, int z, unsigned int channel) const { return get_voxel(x, y, z, channel); }
	_FORCE_INLINE_ void _b_set_voxel(int value, int x, int y, int z, unsigned int channel) { set_voxel(value, x, y, z, channel); }
	void _b_copy_from(Ref<VoxelBuffer> other, unsigned int channel);
	void _b_copy_from_area(Ref<VoxelBuffer> other, Vector3 src_min, Vector3 src_max, Vector3 dst_min, unsigned int channel);
	_FORCE_INLINE_ void _b_fill_area(int defval, Vector3 min, Vector3 max, unsigned int channel_index) { fill_area(defval, Vector3i(min), Vector3i(max), channel_index); }
	_FORCE_INLINE_ void _b_set_voxel_f(real_t value, int x, int y, int z, unsigned int channel) { set_voxel_f(value, x, y, z, channel); }
	void _b_set_voxel_v(int value, Vector3 pos, unsigned int channel_index = 0) { set_voxel(value, pos.x, pos.y, pos.z, channel_index); }

private	struct Channel {
		// Allocated when the channel is populated.
		// Flat array, in order [z][x][y] because it allows faster vertical-wise access (the engine is Y-up).
		uint8_t *data;

		// Default value when data is null
		uint8_t defval;

		Channel() :
				data(NULL),
				defval(0) {}
	};

	// Each channel can store arbitary data.
	// For example, you can decide to store colors (R, G, B, A), gameplay types (type, state, light) or both.
	Channel _channels[MAX_CHANNELS];

	// How many voxels are there in the three directions. All populated channels have the same size.
	Vector3i _size;
};
*/
}