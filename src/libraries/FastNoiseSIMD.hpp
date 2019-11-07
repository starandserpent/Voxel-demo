#include <Godot.hpp>

#include <Reference.hpp>

class FastNoiseSIMD : public godot::GodotScript<godot::Reference> {
	GODOT_CLASS(FastNoiseSIMD)
	
	godot::String data;
public:

	static void _register_methods();

	void _init();

	godot::String get_data() const;
};