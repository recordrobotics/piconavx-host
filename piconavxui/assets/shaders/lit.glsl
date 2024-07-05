#version 330 core
in vec3 fNormal;
in vec3 fPos;
in vec2 fTexCoords;

struct Material {
    sampler2D diffuse;
    vec3 diffuse_tint;
    sampler2D specular;
    vec3 specular_tint;
    float smoothness;
};

struct Light {
    vec3 position;
    vec3 ambient;
    vec3 color;
};

uniform Material material;
uniform Light light;
uniform vec3 viewPos;

out vec4 FragColor;

void main()
{
    vec3 diffuseColor = material.diffuse_tint * texture(material.diffuse, fTexCoords).rgb;
    vec3 specularColor = material.specular_tint * texture(material.specular, fTexCoords).rgb;

    vec3 ambient = light.ambient * diffuseColor;

    vec3 norm = normalize(fNormal);
    vec3 lightDirection = normalize(light.position - fPos);
    float diff = max(dot(norm, lightDirection), 0.0);
    vec3 diffuse = light.color * diff * diffuseColor;

    vec3 viewDirection = normalize(viewPos - fPos);
    vec3 reflectDirection = reflect(-lightDirection, norm);
    float spec = pow(max(dot(viewDirection, reflectDirection), 0.0), material.smoothness * 90.0);
    vec3 specular = light.color * spec * specularColor;

    vec3 result = ambient + diffuse + specular;
    FragColor = vec4(result, 1.0);
}