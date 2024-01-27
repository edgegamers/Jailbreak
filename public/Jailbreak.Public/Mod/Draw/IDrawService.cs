namespace Jailbreak.Public.Mod.Draw;

public interface IDrawService {
   void DrawShape(DrawableShape shape, float tickRate = 0f);

   List<DrawableShape> GetShapes(); 
}