namespace black_dev_tools {
    public struct Vector2Int {
        public Vector2Int(int x, int y) {
            this.x = x;
            this.y = y;
        }
        public static bool operator ==(Vector2Int lhs, Vector2Int rhs) {
            return lhs.Equals(rhs);
        }
        public static bool operator !=(Vector2Int lhs, Vector2Int rhs) {
            return !(lhs == rhs);
        }
        public override string ToString() {
            return $"({x},{y})";
        }
        public override bool Equals(object obj) {
            //Check for null and compare run-time types.
            if ((obj == null) || !GetType().Equals(obj.GetType())) {
                return false;
            } else {
                Vector2Int p = (Vector2Int)obj;
                return (x == p.x) && (y == p.y);
            }
        }
        public override int GetHashCode() {
            return (x << 2) ^ y;
        }
        public int x;
        public int y;
    }
}