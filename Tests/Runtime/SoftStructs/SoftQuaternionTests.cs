using NUnit.Framework;

namespace GameLibrary.Mathematics.Tests
{
    public class SoftQuaternionTests
    {
        [Test]
        public static void DotProductShouldBeOneOrNegativeOneOnSameRotations()
        {
            // Arrange
            SoftQuaternionGeneral rotation180 = new SoftQuaternionGeneral(SoftFloat.One, SoftFloat.Zero, SoftFloat.Zero, SoftFloat.Zero);
            SoftQuaternionGeneral rotationMinus180 = new SoftQuaternionGeneral(-SoftFloat.One, SoftFloat.Zero, SoftFloat.Zero, SoftFloat.Zero);
            
            // Act
            SoftFloat negativeDot = SoftQuaternionGeneral.Dot(rotation180, rotationMinus180);
            SoftFloat dot = SoftQuaternionGeneral.Dot(rotation180, rotation180);
            
            // Assert
            Assert.IsTrue(SoftMath.ApproximatelyEqual(negativeDot, SoftFloat.MinusOne));
            Assert.IsTrue(SoftMath.ApproximatelyEqual(dot, SoftFloat.One));
        }

        [Test]
        public static void MultiplicationWithVectorShouldRotateVector()
        {
            // Arrange
            SoftVector3 point = SoftVector3.Up;
            SoftQuaternionGeneral rotation180 = new SoftQuaternionGeneral(SoftFloat.One, SoftFloat.Zero, SoftFloat.Zero, SoftFloat.Zero);
            
            // Act
            SoftVector3 transformed = rotation180 * point;
            
            // Assert
            Assert.IsTrue(transformed == SoftVector3.Down);
        }
        
        [Test]
        public static void MultiplicationWithVectorShouldRotateAndScaleVector()
        {
            // Arrange
            SoftVector3 point = SoftVector3.Up;
            SoftQuaternionGeneral rotation180 = new SoftQuaternionGeneral(SoftFloat.One, SoftFloat.Zero, SoftFloat.Zero, SoftFloat.Zero);
            SoftQuaternionGeneral scaling9 = new SoftQuaternionGeneral(SoftFloat.Zero, SoftFloat.Zero, SoftFloat.Zero, (SoftFloat)3f);

            // Act
            SoftVector3 transformed = scaling9 * rotation180 * point;
            
            // Assert
            Assert.IsTrue(transformed == SoftVector3.Down * (SoftFloat)9f);
        }
    }
}