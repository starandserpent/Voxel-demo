using Math3D;

namespace Util{
public class ArraySlice
{
    private Vector3i PTR;
    private Vector3i Size;
	public ArraySlice(Vector3i p_ptr, Vector3i p_begin, Vector3i p_end) {
		PTR = p_ptr + p_begin;
		Size = p_end - p_begin;
    }

	private Vector3i GetSize() {
		return Size;
	}

}
}