-- Slasher Bear Trap
local s,id=GetID()
function s.initial_effect(c)
    c:EnableCounterPermit(0x199)
    -- Activate on opponent's summon
    local e1=Effect.CreateEffect(c)
    e1:SetCategory(CATEGORY_DISABLE+CATEGORY_ATKCHANGE)
    e1:SetType(EFFECT_TYPE_ACTIVATE)
    e1:SetCode(EVENT_SUMMON_SUCCESS)
    e1:SetProperty(EFFECT_FLAG_CARD_TARGET)
    e1:SetCondition(s.condition)
    e1:SetTarget(s.target)
    e1:SetOperation(s.operation)
    c:RegisterEffect(e1)
    local e2=e1:Clone()
    e2:SetCode(EVENT_SPSUMMON_SUCCESS)
    c:RegisterEffect(e2)
    local e3=e1:Clone()
    e3:SetCode(EVENT_FLIP_SUMMON_SUCCESS)
    c:RegisterEffect(e3)
    
    -- Destroy target(s) when this card leaves the field
    local e4=Effect.CreateEffect(c)
    e4:SetType(EFFECT_TYPE_CONTINUOUS+EFFECT_TYPE_SINGLE)
    e4:SetCode(EVENT_LEAVE_FIELD)
    e4:SetOperation(s.desop)
    c:RegisterEffect(e4)
end

-- Condition: Did the opponent summon?
function s.condition(e,tp,eg,ep,ev,re,r,rp)
    return eg:IsExists(Card.IsControler,1,nil,1-tp)
end

-- Target: Select the summoned monster(s)
function s.target(e,tp,eg,ep,ev,re,r,rp,chk,chkc)
    if chkc then return eg:IsContains(chkc) and chkc:IsControler(1-tp) end
    if chk==0 then return eg:IsExists(Card.IsCanBeEffectTarget,1,nil,e) end
    local g=eg:Filter(Card.IsCanBeEffectTarget,nil,e):Filter(Card.IsControler,nil,1-tp)
    Duel.SetTargetCard(g)
end

-- Operation: Apply the debuffs and link the cards together
function s.operation(e,tp,eg,ep,ev,re,r,rp)
    local c=e:GetHandler()
    if not c:IsRelateToEffect(e) then return end
    local g=Duel.GetTargetCards(e)
    
    for tc in aux.Next(g) do
        c:SetCardTarget(tc)
        
        -- Negate effects
        local e1=Effect.CreateEffect(c)
        e1:SetType(EFFECT_TYPE_SINGLE)
        e1:SetCode(EFFECT_DISABLE)
        e1:SetReset(RESET_EVENT+RESETS_STANDARD)
        e1:SetCondition(s.rcon)
        tc:RegisterEffect(e1)
        local e2=e1:Clone()
        e2:SetCode(EFFECT_DISABLE_EFFECT)
        tc:RegisterEffect(e2)
        
        -- ATK becomes 0
        local e3=Effect.CreateEffect(c)
        e3:SetType(EFFECT_TYPE_SINGLE)
        e3:SetCode(EFFECT_SET_ATTACK_FINAL)
        e3:SetValue(0)
        e3:SetReset(RESET_EVENT+RESETS_STANDARD)
        e3:SetCondition(s.rcon)
        tc:RegisterEffect(e3)
        
        -- Cannot attack
        local e4=Effect.CreateEffect(c)
        e4:SetType(EFFECT_TYPE_SINGLE)
        e4:SetCode(EFFECT_CANNOT_ATTACK)
        e4:SetReset(RESET_EVENT+RESETS_STANDARD)
        e4:SetCondition(s.rcon)
        tc:RegisterEffect(e4)
        
        -- Destroy THIS card if the monster leaves the field
        local e5=Effect.CreateEffect(c)
        e5:SetType(EFFECT_TYPE_CONTINUOUS+EFFECT_TYPE_FIELD)
        e5:SetRange(LOCATION_SZONE)
        e5:SetCode(EVENT_LEAVE_FIELD)
        e5:SetCondition(s.descon2)
        e5:SetOperation(s.desop2)
        e5:SetLabelObject(tc)
        c:RegisterEffect(e5)
    end
end

-- Maintenance Condition: Ensures the effects only apply while the trap targets the monster
function s.rcon(e)
    return e:GetOwner():IsHasCardTarget(e:GetHandler())
end

-- Destroy target when this trap leaves
function s.desop(e,tp,eg,ep,ev,re,r,rp)
    local c=e:GetHandler()
    local g=c:GetCardTarget()
    if g and #g>0 then
        Duel.Destroy(g,REASON_EFFECT)
    end
end

-- Condition to destroy this trap when the target leaves
function s.descon2(e,tp,eg,ep,ev,re,r,rp)
    local tc=e:GetLabelObject()
    return eg:IsContains(tc)
end

-- Operation to destroy this trap
function s.desop2(e,tp,eg,ep,ev,re,r,rp)
    Duel.Destroy(e:GetHandler(),REASON_EFFECT)
end