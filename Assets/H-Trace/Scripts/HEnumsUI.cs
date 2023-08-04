namespace HTrace
{
	 public enum DenoiserIntensity
        {
            Low = 0,
            Medium = 1,
            High = 2,
        }
        public enum NormalsMode
        {
            RegularNormals = 0,
            BentNormals = 1,
            BentCones = 2,
        }
        public enum ThicknessMode
        {   
            Disabled = 0,
            Standard = 1,
            Accurate = 2,
        }
        public enum ResolutionScale
        {   
            None = 0,
            Checkerboard = 1,
            HalfResolution = 2,
        }
        public enum DebugMode
        {   
            None = 0,
            GlobalIllumination = 1,
            AmbientOcclusion = 2,
            SpecularOcclusion = 3,
            Normals = 4,
        }
        public enum FallbackMode
        {
            ReflectionProbes = 0,
            CustomProbe = 1,
        }
}
