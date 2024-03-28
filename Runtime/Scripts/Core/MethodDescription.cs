namespace DragynGames.Commands
{
    public struct MethodDescription {
        public string name;
        public string[] parameterName;
        public string[] parameterType;

        public override string ToString() {
            string output = name + '(';
            for(int i = 0; i < parameterName.Length; i++) {
                string separator = i == parameterName.Length - 1 ? "" : ",";
                string paramName = parameterName[i].ToLower();
                string paramType = parameterType[i].ToUpper();
                if(paramType.Contains("SYSTEM.")) {
                    paramType = paramType.Remove(0, 7);
                }

                output = $"{output} {paramType} {paramName}";
            }
            output = output + ")";
            return output;
        }
    }
}